using System.Net;
using System.Net.Http.Json;
using Firelink.Platform.Nexus.Models;
using Microsoft.Extensions.Logging;

namespace Firelink.Platform.Nexus;

/// <summary>
/// HTTP-клиент к Nexus API (https://api.nexusmods.com/v1/).
///
/// Заголовки на каждый запрос:
///   apikey: &lt;ключ&gt;
///   Application-Name: Firelink
///   Application-Version: 0.1.0
///   User-Agent: Firelink/0.1.0
///   Accept: application/json
///
/// Обработка ошибок: 401/403/404/429 → InvalidOperationException с внятным
/// текстом. Остальные не-2xx → HttpRequestException.
///
/// IsPremiumAsync кешируется: результат первого успешного запроса
/// запоминается на всё время жизни инстанса (NexusClient — синглтон в DI).
/// Параллельные вызовы получают одну и ту же Task — один HTTP-запрос.
/// Faulted/canceled результат НЕ кешируется: retry должен переспросить.
///
/// НЕ делает retry download-ссылок — это ответственность ArchiveDownloadHelper.
/// НЕ кеширует download-ссылки — они временные.
/// </summary>
public sealed class NexusClient
{
    private const string BaseUrl = "https://api.nexusmods.com/v1";
    private const string ApplicationName = "Firelink";
    private const string ApplicationVersion = "0.1.0";
    private const string UserAgent = "Firelink/0.1.0";

    private readonly HttpClient _http;
    private readonly INexusApiKeyProvider _keyProvider;
    private readonly ILogger<NexusClient> _logger;

    private readonly object _isPremiumLock = new();
    private Task<bool>? _isPremiumTask;

    public NexusClient(
        HttpClient http,
        INexusApiKeyProvider keyProvider,
        ILogger<NexusClient> logger)
    {
        _http = http;
        _keyProvider = keyProvider;
        _logger = logger;
    }

    /// <summary>
    /// Проверяет, является ли текущий API-ключ Premium-аккаунтом.
    ///
    /// Результат кешируется на время жизни клиента: первый успешный
    /// запрос к /validate.json запоминается, все последующие вызовы
    /// возвращают его же. Параллельные вызовы ждут одну Task — один
    /// HTTP-запрос на весь pipeline.
    ///
    /// Если предыдущий запрос был отменён или упал — следующий вызов
    /// сделает новый. Это важно для retry: ArchiveDownloadHelper
    /// повторит DownloadAsync, и мы не должны «отравить» его
    /// faulted-результатом.
    /// </summary>
    public async Task<bool> IsPremiumAsync(CancellationToken ct)
    {
        Task<bool> task;

        lock (_isPremiumLock)
        {
            if (_isPremiumTask is null ||
                _isPremiumTask.IsFaulted ||
                _isPremiumTask.IsCanceled)
            {
                _isPremiumTask = FetchIsPremiumAsync(ct);
            }

            task = _isPremiumTask;
        }

        return await task.ConfigureAwait(false);
    }

    private async Task<bool> FetchIsPremiumAsync(CancellationToken ct)
    {
        var validate = await SendAsync<NexusValidateResponse>(
            "/users/validate.json",
            notFoundMessage: "Nexus rejected the API key: not a valid account.",
            ct).ConfigureAwait(false);

        _logger.LogDebug(
            "Nexus validate: name={Name}, premium={IsPremium}",
            validate.Name ?? "<unknown>", validate.IsPremium);

        return validate.IsPremium;
    }

    /// <summary>
    /// Возвращает список CDN-ссылок для скачивания файла.
    /// Обычно их несколько (разные CDN-ноды). NexusDownloader перебирает
    /// их по очереди.
    /// </summary>
    public async Task<IReadOnlyList<NexusDownloadLink>> GetDownloadLinksAsync(
        string game, int modId, int fileId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(game))
            throw new ArgumentException("Game domain must be non-empty.", nameof(game));
        if (modId <= 0)
            throw new ArgumentOutOfRangeException(nameof(modId), "modId must be positive.");
        if (fileId <= 0)
            throw new ArgumentOutOfRangeException(nameof(fileId), "fileId must be positive.");

        var path = $"/games/{Uri.EscapeDataString(game)}/mods/{modId}/files/{fileId}/download_link.json";

        var links = await SendAsync<NexusDownloadLink[]>(
            path,
            notFoundMessage: $"Nexus mod/file not found: {game}/{modId}/{fileId}.",
            ct).ConfigureAwait(false);

        // Nexus иногда возвращает null-элементы или элементы без URI —
        // фильтруем сразу, чтобы NexusDownloader не проверял.
        var usable = links
            .Where(l => l?.Uri is not null)
            .ToArray();

        if (usable.Length != links.Length)
        {
            _logger.LogWarning(
                "Nexus returned {Total} download links for {Game}/{Mod}/{File}, " +
                "{Usable} of them usable",
                links.Length, game, modId, fileId, usable.Length);
        }

        return usable;
    }

    // ------------------------------------------------------------------
    //  HTTP
    // ------------------------------------------------------------------

    private async Task<T> SendAsync<T>(
        string relativePath,
        string notFoundMessage,
        CancellationToken ct)
    {
        var key = _keyProvider.TryGetApiKey();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Nexus API key not found. Put your key in " +
                "%USERPROFILE%\\.firelink\\nexus.key " +
                "(one line, no quotes).");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get, BaseUrl + relativePath);

        request.Headers.Add("apikey", key);
        request.Headers.Add("Application-Name", ApplicationName);
        request.Headers.Add("Application-Version", ApplicationVersion);
        request.Headers.Add("User-Agent", UserAgent);
        request.Headers.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        _logger.LogDebug("Nexus API GET {Path}", relativePath);

        using var response = await _http.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content
                .ReadFromJsonAsync<T>(cancellationToken: ct).ConfigureAwait(false);

            if (result is null)
            {
                throw new InvalidOperationException(
                    $"Nexus API returned an empty body for {relativePath}.");
            }

            return result;
        }

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                throw new InvalidOperationException(
                    "Nexus API key is invalid or revoked. " +
                    "Check %USERPROFILE%\\.firelink\\nexus.key.");

            case HttpStatusCode.Forbidden:
                throw new InvalidOperationException(
                    "Nexus Premium is required for automatic downloads. " +
                    "Log in with a Premium account or download manually.");

            case HttpStatusCode.NotFound:
                throw new InvalidOperationException(notFoundMessage);

            case HttpStatusCode.TooManyRequests:
                throw new InvalidOperationException(
                    "Nexus API rate limit exceeded. Wait a minute and retry.");

            default:
                throw new HttpRequestException(
                    $"Nexus API returned {(int)response.StatusCode} " +
                    $"({response.ReasonPhrase}) for {relativePath}.");
        }
    }
}
