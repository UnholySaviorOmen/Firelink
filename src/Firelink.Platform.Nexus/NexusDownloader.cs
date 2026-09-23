using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Sources;
using Microsoft.Extensions.Logging;

namespace Firelink.Platform.Nexus;

/// <summary>
/// Скачивание архивов с Nexus Mods через официальный API.
///
/// Логика:
///   1. Проверить, что аккаунт Premium.
///   2. Получить список CDN-ссылок через NexusClient.
///   3. Перебрать ссылки по очереди.
///   4. Вернуть Stream с содержимым архива.
///
/// HttpClient для CDN берётся из IHttpClientFactory по имени "nexus"
/// (таймаут 10 минут). Это критично для больших архивов — у Nexus
/// есть моды на 3+ ГБ.
///
/// Скачивание идёт в TempFileStream (временный файл на диске), а не в
/// MemoryStream: MemoryStream не держит больше ~2 ГБ.
///
/// Retry / hash-check / .part — на стороне ArchiveDownloadHelper.
/// </summary>
public sealed class NexusDownloader : IArchiveDownloader
{
    public const string HttpClientName = "nexus";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NexusClient _client;
    private readonly ILogger<NexusDownloader> _logger;

    public NexusDownloader(
        IHttpClientFactory httpClientFactory,
        NexusClient client,
        ILogger<NexusDownloader> logger)
    {
        _httpClientFactory = httpClientFactory;
        _client = client;
        _logger = logger;
    }

    public string SourceType => "nexus";

    public async Task<Stream> DownloadAsync(
        ArchiveSourceRef source, CancellationToken ct)
    {
        if (source is not NexusSourceRef nexus)
        {
            throw new ArgumentException(
                $"Expected NexusSourceRef, got {source.GetType().Name}.",
                nameof(source));
        }

        ct.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Downloading from Nexus: {Game}/{ModId}/{FileId}",
            nexus.Game, nexus.ModId, nexus.FileId);

        // 1. Premium-проверка (кешируется в NexusClient: один запрос
        // на весь pipeline, сколько бы архивов ни качалось).
        var isPremium = await _client.IsPremiumAsync(ct);

        ct.ThrowIfCancellationRequested();
        if (!isPremium)
        {
            throw new InvalidOperationException(
                "Nexus Premium is required for automatic downloads. " +
                $"Mod {nexus.Game}/{nexus.ModId}/{nexus.FileId} can only be " +
                "downloaded manually from the Nexus website.");
        }

        // 2. Получаем список CDN-нод.
        var links = await _client.GetDownloadLinksAsync(
            nexus.Game, nexus.ModId, nexus.FileId, ct);

        if (links.Count == 0)
        {
            throw new InvalidOperationException(
                $"Nexus returned no download links for " +
                $"{nexus.Game}/{nexus.ModId}/{nexus.FileId}. " +
                "The file may be hidden, archived, or restricted.");
        }

        _logger.LogDebug(
            "Nexus returned {Count} CDN node(s) for {Game}/{ModId}/{FileId}",
            links.Count, nexus.Game, nexus.ModId, nexus.FileId);

        // 3. Перебираем по очереди.
        var http = _httpClientFactory.CreateClient(HttpClientName);
        Exception? lastError = null;

        for (int i = 0; i < links.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var link = links[i];
            var label = string.IsNullOrWhiteSpace(link.Name)
                ? $"#{i + 1}"
                : link.Name;

            try
            {
                var stream = await DownloadFromCdnAsync(http, link.Uri!, ct);

                _logger.LogDebug(
                    "Nexus CDN node {Label} succeeded for {Game}/{ModId}/{FileId}",
                    label, nexus.Game, nexus.ModId, nexus.FileId);

                return stream;
            }
            catch (OperationCanceledException)
            {
                // Отмена — не «ошибка CDN». Пробрасываем как есть.
                throw;
            }
            catch (Exception ex)
            {
                lastError = ex;

                _logger.LogWarning(ex,
                    "Nexus CDN node {Label} failed for {Game}/{ModId}/{FileId}: {Message}",
                    label, nexus.Game, nexus.ModId, nexus.FileId, ex.Message);
            }
        }

        // 4. Все ноды упали.
        throw new InvalidOperationException(
            $"All {links.Count} Nexus CDN nodes failed for " +
            $"{nexus.Game}/{nexus.ModId}/{nexus.FileId}: " +
            $"{lastError?.Message ?? "unknown error"}",
            lastError);
    }

    // ------------------------------------------------------------------
    //  CDN
    // ------------------------------------------------------------------

    /// <summary>
    /// Скачивает одну CDN-ноду во временный файл. Возвращает TempFileStream
    /// с Position=0.
    ///
    /// Hash не проверяем — это делает ArchiveDownloadHelper по archive.Hash.
    /// Пишем не в MemoryStream, а в TempFileStream: архивы бывают > 2 ГБ.
    /// </summary>
    private static async Task<Stream> DownloadFromCdnAsync(
        HttpClient http, Uri url, CancellationToken ct)
    {
        using var response = await http.GetAsync(
            url, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Nexus CDN returned {(int)response.StatusCode} " +
                $"({response.ReasonPhrase}) for {url.Host}.");
        }

        var temp = new TempFileStream();

        try
        {
            await using (var networkStream = await response.Content
                .ReadAsStreamAsync(ct))
            {
                await networkStream.CopyToAsync(temp, ct);
            }

            temp.Position = 0;
            return temp;
        }
        catch
        {
            await temp.DisposeAsync();
            throw;
        }
    }
}
