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
/// </summary>
public sealed class MatchStep : IStep<MatchStep.Input, MatchResult>
{
    private const long MaxInlineFileSize = 2L * 1024 * 1024;   // 2 МБ

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
        // 1. Индекс: hash → (archiveId, relativePath, size)
        var index = BuildArchiveIndex(input, ct);

        _logger.LogInformation(
            "Archive index built: {Count} unique hashes from {Archives} archives",
            index.Count, input.ArchiveIndex.Resolved.Count);

        // 2. Сопоставление по модам
        var modDirectives = new Dictionary<string, IReadOnlyList<Directive>>(StringComparer.Ordinal);
        var inlineFiles = new List<InlineFileContent>();
        var orphans = new List<OrphanFile>();

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

        return Task.FromResult(new MatchResult
        {
            ModDirectives = modDirectives,
            InlineFiles = inlineFiles,
            Orphans = orphans,
        });
    }

    private Dictionary<XxHash64Value, IndexEntry> BuildArchiveIndex(Input input, CancellationToken ct)
    {
        var index = new Dictionary<XxHash64Value, IndexEntry>();
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

                // First-wins: если hash уже есть, оставляем первый
                index.TryAdd(hash, new IndexEntry
                {
                    ArchiveId = archive.Id,
                    RelativePath = relativePath,
                    Size = size,
                });
            }
        }

        return index;
    }

    private sealed class IndexEntry
    {
        public required string ArchiveId { get; init; }
        public required string RelativePath { get; init; }
        public required long Size { get; init; }
    }

    public sealed class Input
    {
        public required ArchiveIndex ArchiveIndex { get; init; }
        public required ModScanResult ModScan { get; init; }
        public required string DownloadsPath { get; init; }
        public required string ModsPath { get; init; }
    }
}
