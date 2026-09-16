using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Directives;
using Firelink.Core.Models.Pack;
using Firelink.Platform.MO2.Readers;
using Microsoft.Extensions.Logging;

namespace Firelink.Pack.Steps;

/// <summary>
/// Сопоставляет файлы модов с файлами внутри архивов.
///
/// Логика матчинга:
///  1. Точное совпадение (hash, path) — идеальный случай.
///  2. Совпадение hash, путь отличается (например .mohidden-суффикс) —
///     берётся первый IndexEntry по (archiveId, relativePath) для детерминизма;
///     destination = путь в моде, source = путь в архиве.
///  3. Совпадения нет — файл попадает в Unmatched и выгружается
///     в __Firelink_Output/mods/&lt;ModName&gt;/&lt;relativePath&gt;.
///
/// meta.ini (в корне мода) читается через MetaIniReader и кладётся
/// в MatchResult.ModMetas. В Unmatched и в директивы он не попадает.
///
/// Никаких base64-inline и orphan-лимитов: всё, что не восстановимо
/// из архивов, идёт в __Firelink_Output — автор решает, делать ли патч.
/// </summary>
public sealed class MatchStep : IStep<MatchStep.Input, MatchResult>
{
    private const string MetaIniFileName = "meta.ini";
    private const int InlineDiagnosticsTopN = 20;
    private const string FirelinkOutputModsSubdir = "mods";

    private readonly IArchiveExtractor _extractor;
    private readonly FileHashCache _hashCache;
    private readonly ILogger<MatchStep> _logger;

    public MatchStep(
        IArchiveExtractor extractor,
        FileHashCache hashCache,
        ILogger<MatchStep> logger)
    {
        _extractor = extractor;
        _hashCache = hashCache;
        _logger = logger;
    }

    public Task<MatchResult> ExecuteAsync(Input input, CancellationToken ct)
    {
        // 1. Готовим __Firelink_Output/mods.
        var outputModsRoot = PrepareFirelinkOutput(input.FirelinkOutputPath);

        // 2. Строим индексы.
        var indexes = BuildArchiveIndexes(input, ct);

        _logger.LogInformation(
            "Archive index built: {Hashes} unique hashes, {HashPaths} unique (hash,path), {Paths} unique paths across {Archives} archives",
            indexes.ByHash.Count, indexes.ByHashPath.Count, indexes.ByPath.Count,
            input.ArchiveIndex.Resolved.Count);

        // 3. Обход модов.
        var modDirectives = new Dictionary<string, IReadOnlyList<Directive>>(StringComparer.Ordinal);
        var modMetas = new Dictionary<string, ModMeta>(StringComparer.Ordinal);
        var unmatched = new List<UnmatchedFile>();

        int totalFiles = 0;
        int matchedExact = 0;
        int matchedByHash = 0;
        int unmatchedCount = 0;
        int metaIniCount = 0;

        foreach (var (modName, files) in input.ModScan.Mods)
        {
            ct.ThrowIfCancellationRequested();

            var directives = new List<Directive>(files.Count);
            var modPath = Path.Combine(input.ModsPath, modName);

            foreach (var file in files)
            {
                totalFiles++;

                // meta.ini в корне мода — отдельная сущность.
                if (IsRootMetaIni(file.RelativePath))
                {
                    var metaIniPath = Path.Combine(modPath, MetaIniFileName);
                    var modMeta = MetaIniReader.TryRead(metaIniPath);
                    if (modMeta is not null)
                    {
                        modMetas[modName] = modMeta;
                        metaIniCount++;
                    }
                    continue;
                }

                var directive = TryMatch(file, modName, indexes, out var matchKind);
                if (directive is not null)
                {
                    directives.Add(directive);
                    switch (matchKind)
                    {
                        case MatchKind.Exact:
                            matchedExact++;
                            break;
                        case MatchKind.ByHash:
                            matchedByHash++;
                            break;
                    }
                    continue;
                }

                // Не сматчилось — выгружаем в __Firelink_Output.
                unmatchedCount++;
                unmatched.Add(new UnmatchedFile(
                    ModName: modName,
                    RelativePath: file.RelativePath,
                    Size: file.Size));

                WriteUnmatchedFile(input.ModsPath, outputModsRoot, modName, file.RelativePath);
            }

            modDirectives[modName] = directives;
        }

        _logger.LogInformation(
            "Match complete: {Total} files, {Exact} matched (exact), {ByHash} matched (by hash), {Unmatched} unmatched, {MetaIni} meta.ini",
            totalFiles, matchedExact, matchedByHash, unmatchedCount, metaIniCount);

        if (unmatchedCount > 0)
        {
            _logger.LogInformation(
                "Unmatched files written to __Firelink_Output: {Count} ({Root})",
                unmatchedCount, outputModsRoot);

            LogInlineDiagnostics(unmatched, indexes.ByPath);
        }

        return Task.FromResult(new MatchResult
        {
            ModDirectives = modDirectives,
            Unmatched = unmatched,
            ModMetas = modMetas,
        });
    }

    // ------------------------------------------------------------------
    //  Матчинг
    // ------------------------------------------------------------------

    private FromArchiveDirective? TryMatch(
        ScannedFile file,
        string modName,
        ArchiveIndexes indexes,
        out MatchKind kind)
    {
        // 1. Точное (hash, path).
        if (indexes.ByHashPath.TryGetValue((file.Hash, file.RelativePath), out var exact))
        {
            kind = MatchKind.Exact;
            return new FromArchiveDirective
            {
                Archive = exact.ArchiveId,
                Source = exact.RelativePath,
                Destination = file.RelativePath,
                Hash = file.Hash,
                Size = file.Size,
            };
        }

        // 2. По хешу, путь отличается.
        if (indexes.ByHash.TryGetValue(file.Hash, out var candidates) && candidates.Count > 0)
        {
            var pick = candidates[0];
            kind = MatchKind.ByHash;
            return new FromArchiveDirective
            {
                Archive = pick.ArchiveId,
                Source = pick.RelativePath,
                Destination = file.RelativePath,
                Hash = file.Hash,
                Size = file.Size,
            };
        }

        kind = MatchKind.None;
        return null;
    }

    // ------------------------------------------------------------------
    //  __Firelink_Output
    // ------------------------------------------------------------------

    private string PrepareFirelinkOutput(string firelinkOutputPath)
    {
        var outputModsRoot = Path.Combine(firelinkOutputPath, FirelinkOutputModsSubdir);

        if (Directory.Exists(outputModsRoot))
        {
            try
            {
                Directory.Delete(outputModsRoot, recursive: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to clean __Firelink_Output: {outputModsRoot}", ex);
            }
        }

        Directory.CreateDirectory(outputModsRoot);

        _logger.LogInformation("Firelink output cleaned: {Path}", outputModsRoot);

        return outputModsRoot;
    }

    private void WriteUnmatchedFile(
        string modsRoot,
        string outputModsRoot,
        string modName,
        string relativePath)
    {
        var srcPath = Path.Combine(modsRoot, modName,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        var dstPath = Path.Combine(outputModsRoot, modName,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            var dstDir = Path.GetDirectoryName(dstPath);
            if (!string.IsNullOrEmpty(dstDir))
                Directory.CreateDirectory(dstDir);

            File.Copy(srcPath, dstPath, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to write unmatched file '{Mod}/{Path}' to __Firelink_Output",
                modName, relativePath);
        }
    }

    // ------------------------------------------------------------------
    //  Индексы
    // ------------------------------------------------------------------

    private ArchiveIndexes BuildArchiveIndexes(Input input, CancellationToken ct)
    {
        var byHashPath = new Dictionary<(XxHash64Value, string), IndexEntry>();
        var byHash = new Dictionary<XxHash64Value, List<IndexEntry>>();
        var byPath = new Dictionary<string, Dictionary<XxHash64Value, string>>(
            StringComparer.OrdinalIgnoreCase);

        int processed = 0;
        int total = input.ArchiveIndex.Resolved.Count;

        foreach (var archive in input.ArchiveIndex.Resolved)
        {
            ct.ThrowIfCancellationRequested();

            processed++;
            if (processed % 10 == 0 || processed == total)
            {
                _logger.LogInformation(
                    "Extracting archive {Current}/{Total}: {Name}",
                    processed, total, archive.Name);
            }

            var archivePath = Path.Combine(input.DownloadsPath, archive.Name);

            if (!File.Exists(archivePath))
            {
                _logger.LogWarning(
                    "Archive file not found (skipping): {Path}", archivePath);
                continue;
            }

            using var workspace = new TempWorkspace();

            IReadOnlyList<string> extractedFiles;
            try
            {
                extractedFiles = _extractor.ExtractAsync(archivePath, workspace.Path, ct)
                    .GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to extract archive '{Name}'. Skipping.",
                    archive.Name);
                continue;
            }

            foreach (var relativePath in extractedFiles)
            {
                var fullPath = Path.Combine(workspace.Path,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));

                XxHash64Value hash;
                long size;
                try
                {
                    hash = _hashCache.GetOrCompute(fullPath);
                    size = new FileInfo(fullPath).Length;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to hash '{Path}' from archive '{Archive}'",
                        relativePath, archive.Name);
                    continue;
                }

                var entry = new IndexEntry
                {
                    ArchiveId = archive.Id,
                    RelativePath = relativePath,
                    Size = size,
                };

                // Точный индекс (hash, path).
                byHashPath.TryAdd((hash, relativePath), entry);

                // Основной индекс: hash → list.
                if (!byHash.TryGetValue(hash, out var list))
                {
                    list = new List<IndexEntry>(1);
                    byHash[hash] = list;
                }
                list.Add(entry);

                // Диагностический индекс: path → { hash → archiveId }.
                if (!byPath.TryGetValue(relativePath, out var hashesByPath))
                {
                    hashesByPath = new Dictionary<XxHash64Value, string>();
                    byPath[relativePath] = hashesByPath;
                }
                hashesByPath.TryAdd(hash, archive.Id);
            }
        }

        // Детерминизм: сортируем списки по (archiveId, relativePath).
        foreach (var list in byHash.Values)
        {
            list.Sort(static (a, b) =>
            {
                var c = string.CompareOrdinal(a.ArchiveId, b.ArchiveId);
                if (c != 0) return c;
                return string.CompareOrdinal(a.RelativePath, b.RelativePath);
            });
        }

        return new ArchiveIndexes(byHashPath, byHash, byPath);
    }

    // ------------------------------------------------------------------
    //  Диагностика
    // ------------------------------------------------------------------

    private void LogInlineDiagnostics(
        IReadOnlyList<UnmatchedFile> records,
        IReadOnlyDictionary<string, Dictionary<XxHash64Value, string>> pathIndex)
    {
        _logger.LogInformation(
            "Unmatched diagnostics: {Count} files across {Mods} mods",
            records.Count,
            records.Select(r => r.ModName).Distinct(StringComparer.Ordinal).Count());

        var resolved = new List<ResolvedUnmatched>(records.Count);
        foreach (var rec in records)
        {
            var (reason, archiveId) = ClassifyUnmatched(rec, pathIndex);
            resolved.Add(new ResolvedUnmatched(rec, reason, archiveId));
        }

        int pathNotFound = resolved.Count(r => r.Reason == UnmatchedReason.PathNotFound);
        int hashDiffers = resolved.Count(r => r.Reason == UnmatchedReason.HashDiffers);

        _logger.LogInformation(
            "  by reason: path-not-found = {PathNotFound}, hash-differs = {HashDiffers}",
            pathNotFound, hashDiffers);

        _logger.LogInformation("  --- top {N} ---", InlineDiagnosticsTopN);

        int shown = 0;
        foreach (var item in resolved)
        {
            if (shown >= InlineDiagnosticsTopN) break;
            shown++;

            var rec = item.Record;
            _logger.LogInformation(
                "  {Index}. {Mod} / {Path}",
                shown, rec.ModName, rec.RelativePath);

            switch (item.Reason)
            {
                case UnmatchedReason.PathNotFound:
                    _logger.LogInformation("      reason: path-not-found");
                    break;

                case UnmatchedReason.HashDiffers:
                    _logger.LogInformation(
                        "      reason: hash-differs (present in '{Archive}' as '{Path}')",
                        item.ArchiveId ?? "<unknown>", rec.RelativePath);
                    break;
            }
        }

        if (resolved.Count > shown)
        {
            _logger.LogInformation(
                "  ({More} more unmatched files not shown; counts by reason above)",
                resolved.Count - shown);
        }

        _logger.LogInformation("  by mod:");

        var byMod = resolved
            .GroupBy(r => r.Record.ModName, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal);

        foreach (var g in byMod)
        {
            int mPnf = g.Count(r => r.Reason == UnmatchedReason.PathNotFound);
            int mHd = g.Count(r => r.Reason == UnmatchedReason.HashDiffers);

            _logger.LogInformation(
                "    {Mod,-40} : {Total,3} (path-not-found={Pnf}, hash-differs={Hd})",
                g.Key, g.Count(), mPnf, mHd);
        }
    }

    private static (UnmatchedReason Reason, string? ArchiveId) ClassifyUnmatched(
        UnmatchedFile rec,
        IReadOnlyDictionary<string, Dictionary<XxHash64Value, string>> pathIndex)
    {
        if (!pathIndex.TryGetValue(rec.RelativePath, out var hashesByPath) || hashesByPath.Count == 0)
        {
            return (UnmatchedReason.PathNotFound, null);
        }

        // Путь есть. Всё, что не сматчилось по (hash, path) и по hash — hash-differs.
        var firstArchive = hashesByPath.Values.FirstOrDefault();
        return (UnmatchedReason.HashDiffers, firstArchive);
    }

    private static bool IsRootMetaIni(string relativePath)
        => string.Equals(relativePath, MetaIniFileName, StringComparison.OrdinalIgnoreCase);

    // ------------------------------------------------------------------
    //  Типы
    // ------------------------------------------------------------------

    private sealed class IndexEntry
    {
        public required string ArchiveId { get; init; }
        public required string RelativePath { get; init; }
        public required long Size { get; init; }
    }

    private sealed record ArchiveIndexes(
        Dictionary<(XxHash64Value, string), IndexEntry> ByHashPath,
        Dictionary<XxHash64Value, List<IndexEntry>> ByHash,
        Dictionary<string, Dictionary<XxHash64Value, string>> ByPath);

    private enum MatchKind
    {
        None,
        Exact,
        ByHash,
    }

    private enum UnmatchedReason
    {
        PathNotFound,
        HashDiffers,
    }

    private readonly record struct ResolvedUnmatched(
        UnmatchedFile Record,
        UnmatchedReason Reason,
        string? ArchiveId);

    public sealed class Input
    {
        public required ArchiveIndex ArchiveIndex { get; init; }
        public required ModScanResult ModScan { get; init; }
        public required string DownloadsPath { get; init; }
        public required string ModsPath { get; init; }
        public required string FirelinkOutputPath { get; init; }
    }
}
