using System.Collections.Concurrent;
using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest;
using Firelink.Install.Downloaders;
using Microsoft.Extensions.Logging;

namespace Firelink.Install.Steps;

/// <summary>
/// Обеспечивает наличие всех архивов из manifest.Archives в MO2/downloads/.
///
/// Не трогает MO2-архив — это забота BootstrapMo2Step.
///
/// Логика:
///   1. Сканирует downloads/, строит hash → path (один раз).
///   2. Для каждого archive из manifest.Archives:
///        - если hash уже есть → AlreadyPresent.
///        - иначе перебирает sources по порядку:
///            - github → GitHubDownloader (удалён; сейчас только mirror/nexus)
///            - mirror → MirrorDownloader
///            - nexus  → warning, skipped (12.8)
///          3 попытки на источник + Polly backoff (см. ArchiveDownloadHelper).
///          Скачивает в <name>.part, потом File.Move в <name>.
///        - если все источники провалились → исключение.
///
/// Не удаляет ничего из downloads/. Не распаковывает. Не трогает моды.
/// </summary>
public sealed class SyncArchivesStep : IStep<SyncArchivesStep.Input, SyncArchivesStep.Output>
{
    private readonly DownloaderRegistry _downloaders;
    private readonly FileHashCache _hashCache;
    private readonly ILogger<SyncArchivesStep> _logger;

    public SyncArchivesStep(
        DownloaderRegistry downloaders,
        FileHashCache hashCache,
        ILogger<SyncArchivesStep> logger)
    {
        _downloaders = downloaders;
        _hashCache = hashCache;
        _logger = logger;
    }

    public async Task<Output> ExecuteAsync(Input input, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Syncing {Count} archives into {Path}",
            input.Manifest.Archives.Count, input.DownloadsPath);

        var localByHash = ScanDownloads(input.DownloadsPath, ct);
        _logger.LogInformation(
            "Local downloads scan: {Count} files with unique hashes",
            localByHash.Count);

        var results = new ConcurrentBag<ArchiveResult>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = input.ParallelOptions.MaxDegreeOfParallelism,
            CancellationToken = ct,
        };

        await Parallel.ForEachAsync(
            input.Manifest.Archives,
            parallelOptions,
            async (archive, innerCt) =>
            {
                var result = await ProcessOneArchiveAsync(
                    archive, input.DownloadsPath, localByHash, innerCt);
                results.Add(result);
            });

        var alreadyPresent = results
            .Where(r => r.Kind == ArchiveResultKind.AlreadyPresent)
            .Select(r => r.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var downloaded = results
            .Where(r => r.Kind == ArchiveResultKind.Downloaded)
            .Select(r => r.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var skipped = results
            .Where(r => r.Kind == ArchiveResultKind.Skipped)
            .Select(r => r.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _logger.LogInformation(
            "Sync complete: {Already} present, {Downloaded} downloaded, {Skipped} skipped",
            alreadyPresent.Count, downloaded.Count, skipped.Count);

        return new Output
        {
            AlreadyPresent = alreadyPresent,
            Downloaded = downloaded,
            Skipped = skipped,
        };
    }

    // ------------------------------------------------------------------
    //  Сканирование downloads/
    // ------------------------------------------------------------------

    private Dictionary<XxHash64Value, string> ScanDownloads(
        string downloadsPath, CancellationToken ct)
    {
        if (!Directory.Exists(downloadsPath))
        {
            Directory.CreateDirectory(downloadsPath);
        }

        var result = new Dictionary<XxHash64Value, string>();

        var files = Directory.EnumerateFiles(downloadsPath, "*",
            SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            if (file.EndsWith(ArchiveDownloadHelper.PartSuffix,
                StringComparison.OrdinalIgnoreCase))
                continue;

            if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                var hash = _hashCache.GetOrCompute(file);
                result.TryAdd(hash, file);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to hash {File} during downloads scan — skipping",
                    file);
            }
        }

        return result;
    }

    // ------------------------------------------------------------------
    //  Обработка одного архива
    // ------------------------------------------------------------------

    private async Task<ArchiveResult> ProcessOneArchiveAsync(
        ArchiveEntry archive,
        string downloadsPath,
        Dictionary<XxHash64Value, string> localByHash,
        CancellationToken ct)
    {
        if (localByHash.TryGetValue(archive.Hash, out var existingPath))
        {
            _logger.LogDebug(
                "Archive already present: {Name} ({Hash})",
                archive.Name, archive.Hash);
            return ArchiveResult.AlreadyPresent(archive.Name);
        }

        if (archive.Sources.Count == 0)
        {
            throw new InvalidOperationException(
                $"Archive '{archive.Name}' has no sources.");
        }

        var targetPath = Path.Combine(downloadsPath, archive.Name);
        var partPath = targetPath + ArchiveDownloadHelper.PartSuffix;

        bool anySourceSkipped = false;

        foreach (var source in archive.Sources)
        {
            ct.ThrowIfCancellationRequested();

            var sourceType = ArchiveDownloadHelper.GetSourceType(source);
            var downloader = _downloaders.TryGet(sourceType);

            if (downloader is null)
            {
                _logger.LogWarning(
                    "No downloader for source type '{Type}' " +
                    "(archive '{Name}') — skipping this source",
                    sourceType, archive.Name);
                anySourceSkipped = true;
                continue;
            }

            try
            {
                await ArchiveDownloadHelper.DownloadWithRetryAsync(
                    downloader,
                    source,
                    partPath,
                    archive.Hash,
                    displayName: $"archive '{archive.Name}'",
                    hashCache: _hashCache,
                    logger: _logger,
                    ct);

                File.Move(partPath, targetPath, overwrite: true);

                _logger.LogInformation(
                    "Downloaded: {Name} → {Path}",
                    archive.Name, targetPath);

                return ArchiveResult.Downloaded(archive.Name);
            }
            catch (OperationCanceledException)
            {
                ArchiveDownloadHelper.CleanupPartFile(partPath, _logger);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to download '{Name}' from source {Type}: {Message}",
                    archive.Name, sourceType, ex.Message);
                ArchiveDownloadHelper.CleanupPartFile(partPath, _logger);
            }
        }

        var reason = anySourceSkipped
            ? "some sources skipped (nexus not yet supported)"
            : "all sources failed";

        throw new InvalidOperationException(
            $"Failed to sync archive '{archive.Name}' ({archive.Hash}): {reason}.");
    }

    // ------------------------------------------------------------------
    //  Результаты
    // ------------------------------------------------------------------

    private enum ArchiveResultKind
    {
        AlreadyPresent,
        Downloaded,
        Skipped,
    }

    private readonly record struct ArchiveResult(string Name, ArchiveResultKind Kind)
    {
        public static ArchiveResult AlreadyPresent(string name)
            => new(name, ArchiveResultKind.AlreadyPresent);

        public static ArchiveResult Downloaded(string name)
            => new(name, ArchiveResultKind.Downloaded);

        public static ArchiveResult Skipped(string name)
            => new(name, ArchiveResultKind.Skipped);
    }

    // ------------------------------------------------------------------
    //  Input / Output
    // ------------------------------------------------------------------

    public sealed class Input
    {
        public required ModlistManifest Manifest { get; init; }
        public required string DownloadsPath { get; init; }
        public required ParallelOptions ParallelOptions { get; init; }
    }

    public sealed class Output
    {
        public required IReadOnlyList<string> AlreadyPresent { get; init; }
        public required IReadOnlyList<string> Downloaded { get; init; }
        public required IReadOnlyList<string> Skipped { get; init; }
    }
}
