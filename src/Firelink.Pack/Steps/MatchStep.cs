using System.Collections.Concurrent;
using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Directives;
using Firelink.Core.Models.Pack;
using Microsoft.Extensions.Logging;

namespace Firelink.Pack.Steps;

/// <summary>
/// Сопоставляет файлы модов с файлами внутри архивов.
/// Распаковывает архивы по одному через TempWorkspace.
///
/// Дополнительно (диагностика, блок 10.6) строит второй индекс
/// path → { hash → archiveId }, чтобы для каждого inline-файла
/// определить причину: "path not found" или "hash differs".
/// </summary>
public sealed class MatchStep : IStep<MatchStep.Input, MatchResult>
{
    private const long MaxInlineFileSize = 2L * 1024 * 1024;   // 2 МБ
    private const int InlineDiagnosticsTopN = 20;

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
        // 1. Индексы: hash → (archiveId, relativePath, size)
        //             path → { hash → archiveId }  (для диагностики)
        var (index, pathIndex) = BuildArchiveIndexes(input, ct);

        _logger.LogInformation(
            "Archive index built: {Count} unique hashes from {Archives} archives",
            index.Count, input.ArchiveIndex.Resolved.Count);

        _logger.LogInformation(
            "Archive path index built: {Count} unique paths across {Archives} archives",
            pathIndex.Count, input.ArchiveIndex.Resolved.Count);

        // 2. Сопоставление по модам
        var modDirectives = new Dictionary<string, IReadOnlyList<Directive>>(StringComparer.Ordinal);
        var inlineFiles = new List<InlineFileContent>();
        var orphans = new List<OrphanFile>();

        // Сырые inline-записи для диагностики (вариант A — не идут в MatchResult).
        var inlineDiagnostics = new List<InlineRecord>();

        int inlineCounter = 0;
        int totalFiles = 0;
        int matchedFiles = 0;
        int inlineCount = 0;

        foreach (var (modName, files) in input.ModScan.Mods)
        {
            ct.ThrowIfCancellationRequested();

            var directives = new List<Directive>(files.Count);

            foreach (var file in files)
            {
                totalFiles++;

                if (index.TryGetValue(file.Hash, out var found))
                {
                    matchedFiles++;
                    directives.Add(new FromArchiveDirective
                    {
                        Archive = found.ArchiveId,
                        Source = found.RelativePath,
                        Destination = file.RelativePath,
                        Hash = file.Hash,
                        Size = file.Size,
                    });
                }
                else if (file.Size <= MaxInlineFileSize)
                {
                    inlineCount++;
                    var id = $"inline-{inlineCounter++:D6}";
                    var fullPath = Path.Combine(input.ModsPath, modName,
                        file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                    var content = File.ReadAllBytes(fullPath);

                    inlineFiles.Add(new InlineFileContent
                    {
                        Id = id,
                        Content = content,
                        Hash = file.Hash,
                    });

                    directives.Add(new InlineFileDirective
                    {
                        InlineFileId = id,
                        Destination = file.RelativePath,
                    });

                    // Диагностика: разберёмся, почему не нашли в архивах.
                    inlineDiagnostics.Add(new InlineRecord(
                        ModName: modName,
                        RelativePath: file.RelativePath,
                        Hash: file.Hash));
                }
                else
                {
                    orphans.Add(new OrphanFile
                    {
                        ModName = modName,
                        RelativePath = file.RelativePath,
                        Size = file.Size,
                    });
                }
            }

            modDirectives[modName] = directives;
        }

        _logger.LogInformation(
            "Match complete: {Total} files, {Matched} matched, {Inline} inline, {Orphans} orphans",
            totalFiles, matchedFiles, inlineCount, orphans.Count);

        if (orphans.Count > 0)
        {
            _logger.LogWarning(
                "{Count} orphan files (not in any archive and > {Limit} bytes):",
                orphans.Count, MaxInlineFileSize);
            foreach (var o in orphans.Take(10))
                _logger.LogWarning("  - {Mod}/{Path} ({Size} bytes)",
                    o.ModName, o.RelativePath, o.Size);
            if (orphans.Count > 10)
                _logger.LogWarning("  ... and {More} more", orphans.Count - 10);
        }

        // 3. Диагностика inline-файлов.
        if (inlineDiagnostics.Count > 0)
        {
            LogInlineDiagnostics(inlineDiagnostics, pathIndex);
        }

        return Task.FromResult(new MatchResult
        {
            ModDirectives = modDirectives,
            InlineFiles = inlineFiles,
            Orphans = orphans,
        });
    }

    // ------------------------------------------------------------------
    //  Диагностика
    // ------------------------------------------------------------------

    private void LogInlineDiagnostics(
        IReadOnlyList<InlineRecord> records,
        IReadOnlyDictionary<string, Dictionary<XxHash64Value, string>> pathIndex)
    {
        _logger.LogInformation(
            "Inline diagnostics: {Count} inline files across {Mods} mods",
            records.Count, records.Select(r => r.ModName).Distinct(StringComparer.Ordinal).Count());

        // Разбор причины для каждой записи.
        var resolved = new List<ResolvedInline>(records.Count);
        foreach (var rec in records)
        {
            var (reason, archiveId) = ClassifyInline(rec, pathIndex);
            resolved.Add(new ResolvedInline(rec, reason, archiveId));
        }

        var byReason = resolved
            .GroupBy(r => r.Reason)
            .ToDictionary(g => g.Key, g => g.Count());

        int pathNotFound = byReason.TryGetValue(InlineReason.PathNotFound, out var pnf) ? pnf : 0;
        int hashDiffers = byReason.TryGetValue(InlineReason.HashDiffers, out var hd) ? hd : 0;

        _logger.LogInformation(
            "  by reason: path-not-found = {PathNotFound}, hash-differs = {HashDiffers}",
            pathNotFound, hashDiffers);

        // Топ-N с деталями.
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
                case InlineReason.PathNotFound:
                    _logger.LogInformation("      reason: path-not-found");
                    break;

                case InlineReason.HashDiffers:
                    _logger.LogInformation(
                        "      reason: hash-differs (present in '{Archive}' as '{Path}')",
                        item.ArchiveId ?? "<unknown>", rec.RelativePath);
                    break;
            }
        }

        if (resolved.Count > shown)
        {
            _logger.LogInformation(
                "  ({More} more inline files not shown; counts by reason above)",
                resolved.Count - shown);
        }

        // Разбивка по модам.
        _logger.LogInformation("  by mod:");

        var byMod = resolved
            .GroupBy(r => r.Record.ModName, StringComparer.Ordinal)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal);

        foreach (var g in byMod)
        {
            int mPnf = g.Count(r => r.Reason == InlineReason.PathNotFound);
            int mHd = g.Count(r => r.Reason == InlineReason.HashDiffers);

            _logger.LogInformation(
                "    {Mod,-40} : {Total,3} (path-not-found={Pnf}, hash-differs={Hd})",
                g.Key, g.Count(), mPnf, mHd);
        }
    }

    /// <summary>
    /// Классифицирует inline-файл:
    ///  - PathNotFound — пути нет ни в одном архиве;
    ///  - HashDiffers  — путь есть, но ни один хеш по этому пути не совпал.
    ///    Возвращает archiveId первого архива, где встретился этот путь.
    /// </summary>
    private static (InlineReason Reason, string? ArchiveId) ClassifyInline(
        InlineRecord rec,
        IReadOnlyDictionary<string, Dictionary<XxHash64Value, string>> pathIndex)
    {
        if (!pathIndex.TryGetValue(rec.RelativePath, out var hashesByPath) || hashesByPath.Count == 0)
        {
            return (InlineReason.PathNotFound, null);
        }

        // Путь есть. Совпадает ли хотя бы один хеш?
        if (hashesByPath.ContainsKey(rec.Hash))
        {
            // В теории сюда не должны попадать: если бы хеш совпал — MatchStep бы
            // сматчил файл по основному индексу. Но подстрахуемся.
            // Считаем это как "hash differs" не будем — но и как path-not-found тоже нет.
            // На практике: попадание сюда означает, что между основным и вторым
            // индексом рассинхрон — не должно случаться.
            return (InlineReason.HashDiffers, hashesByPath[rec.Hash]);
        }

        // Ни один хеш не совпал. Берём первый archiveId для информативности.
        var firstArchive = hashesByPath.Values.FirstOrDefault();
        return (InlineReason.HashDiffers, firstArchive);
    }

    // ------------------------------------------------------------------
    //  Построение индексов
    // ------------------------------------------------------------------

    private (Dictionary<XxHash64Value, IndexEntry> ByHash,
             Dictionary<string, Dictionary<XxHash64Value, string>> ByPath)
        BuildArchiveIndexes(Input input, CancellationToken ct)
    {
        var byHash = new Dictionary<XxHash64Value, IndexEntry>();
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

                // Основной индекс: hash → entry (first-wins).
                byHash.TryAdd(hash, new IndexEntry
                {
                    ArchiveId = archive.Id,
                    RelativePath = relativePath,
                    Size = size,
                });

                // Второй индекс: path → { hash → archiveId } (first-wins по хешу).
                if (!byPath.TryGetValue(relativePath, out var hashesByPath))
                {
                    hashesByPath = new Dictionary<XxHash64Value, string>();
                    byPath[relativePath] = hashesByPath;
                }
                hashesByPath.TryAdd(hash, archive.Id);
            }
        }

        return (byHash, byPath);
    }

    // ------------------------------------------------------------------
    //  Типы
    // ------------------------------------------------------------------

    private sealed class IndexEntry
    {
        public required string ArchiveId { get; init; }
        public required string RelativePath { get; init; }
        public required long Size { get; init; }
    }

    private enum InlineReason
    {
        PathNotFound,
        HashDiffers,
    }

    private readonly record struct InlineRecord(
        string ModName,
        string RelativePath,
        XxHash64Value Hash);

    private readonly record struct ResolvedInline(
        InlineRecord Record,
        InlineReason Reason,
        string? ArchiveId);

    public sealed class Input
    {
        public required ArchiveIndex ArchiveIndex { get; init; }
        public required ModScanResult ModScan { get; init; }
        public required string DownloadsPath { get; init; }
        public required string ModsPath { get; init; }
    }
}
