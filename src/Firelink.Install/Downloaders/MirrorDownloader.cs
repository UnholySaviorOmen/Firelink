using Firelink.Core.Abstractions;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Sources;

namespace Firelink.Install.Downloaders;

/// <summary>
/// Скачивание архивов по прямой URL-ссылке (mirror).
///
/// Если в источнике указан hash — downloader его проверяет.
/// Если не указан — проверка на стороне SyncArchivesStep (по archive.Hash).
/// </summary>
public sealed class MirrorDownloader : IArchiveDownloader
{
    private readonly HttpClient _http;

    public MirrorDownloader(HttpClient http)
    {
        _http = http;
    }

    public string SourceType => "mirror";

    public async Task<Stream> DownloadAsync(
        ArchiveSourceRef source, CancellationToken ct)
    {
        if (source is not MirrorSourceRef mirror)
        {
            throw new ArgumentException(
                $"Expected MirrorSourceRef, got {source.GetType().Name}.",
                nameof(source));
        }

        using var response = await _http.GetAsync(
            mirror.Url, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Mirror returned {(int)response.StatusCode} " +
                $"({response.ReasonPhrase}) for {mirror.Url}");
        }

        var ms = new MemoryStream();
        await using (var networkStream = await response.Content.ReadAsStreamAsync(ct))
        {
            await networkStream.CopyToAsync(ms, ct);
        }

        ms.Position = 0;

        // Если в mirror-источнике указан hash — проверяем сразу.
        // Это раннее обнаружение битой загрузки до записи на диск.
        var actualHash = XxHash64Value.FromStream(ms);
        ms.Position = 0;

        if (actualHash != mirror.Hash)
        {
            throw new InvalidOperationException(
                $"Mirror hash mismatch for {mirror.Url}: " +
                $"expected {mirror.Hash}, got {actualHash}.");
        }

        return ms;
    }
}
