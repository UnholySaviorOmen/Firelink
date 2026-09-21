using Firelink.Core.Abstractions;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Sources;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Firelink.Core.Archives;

/// <summary>
/// Общий helper для скачивания архивов с retry-логикой, .part-файлами
/// и проверкой хеша. Используется SyncArchivesStep и BootstrapMo2Step.
///
/// Логика скачивания одинакова:
///   1. Downloader отдаёт Stream.
///   2. Пишем в <target>.part.
///   3. Считаем xxHash64 .part-файла.
///   4. Сверяем с ожидаемым.
///   5. Mismatch → InvalidOperationException → retry.
///
/// File.Move(.part → target) — ответственность вызывающего кода.
/// CleanupPartFile при провале — тоже.
///
/// Никаких информационных логов внутри: сообщение о конкретном шаге
/// (например, «Downloaded SkyUI.7z») — дело вызывающего.
/// </summary>
public static class ArchiveDownloadHelper
{
    public const string PartSuffix = ".part";
    public const int MaxAttemptsPerSource = 3;

    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Тип источника из дискриминатора ArchiveSourceRef.
    /// </summary>
    public static string GetSourceType(ArchiveSourceRef source) => source switch
    {
        NexusSourceRef => "nexus",
        MirrorSourceRef => "mirror",
        _ => source.GetType().Name.ToLowerInvariant(),
    };

    /// <summary>
    /// Скачивает архив в partPath с 3 попытками на источник, Polly backoff,
    /// и проверкой хеша. Бросает InvalidOperationException, если после
    /// всех попыток не удалось получить совпадающий хеш.
    ///
    /// НЕ делает File.Move — вызывающий решает, когда .part стал валидным.
    /// НЕ делает cleanup .part при провале — вызывающий решает.
    /// </summary>
    public static async Task DownloadWithRetryAsync(
        IArchiveDownloader downloader,
        ArchiveSourceRef source,
        string partPath,
        XxHash64Value expectedHash,
        string displayName,
        FileHashCache hashCache,
        ILogger logger,
        CancellationToken ct)
    {
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = MaxAttemptsPerSource - 1,
                Delay = RetryDelay,
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry {Attempt}/{Max} for {DisplayName} from {Source}: {Error}",
                        args.AttemptNumber + 1,
                        MaxAttemptsPerSource,
                        displayName,
                        downloader.SourceType,
                        args.Outcome.Exception?.Message ?? "<no exception>");
                    return ValueTask.CompletedTask;
                },
            })
            .Build();

        await pipeline.ExecuteAsync(async token =>
        {
            await using var stream = await downloader.DownloadAsync(source, token);

            await using var fs = new FileStream(
                partPath, FileMode.Create, FileAccess.Write, FileShare.None);

            await stream.CopyToAsync(fs, token);
            await fs.FlushAsync(token);
            fs.Close();

            var actual = hashCache.GetOrCompute(partPath);
            if (actual != expectedHash)
            {
                throw new InvalidOperationException(
                    $"Hash mismatch for {displayName}: " +
                    $"expected {expectedHash}, got {actual}.");
            }
        }, ct);
    }

    /// <summary>
    /// Удаляет .part-файл, если он есть. Игнорирует ошибки (логирует).
    /// </summary>
    public static void CleanupPartFile(string partPath, ILogger logger)
    {
        try
        {
            if (File.Exists(partPath))
                File.Delete(partPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to clean up partial file: {Path}", partPath);
        }
    }
}
