using Firelink.Core.Abstractions;
using Firelink.Core.Models.Manifest.Sources;

namespace Firelink.Platform.GitHub;

/// <summary>
/// Скачивание архивов из GitHub Releases.
///
/// Используем прямой HTTP GET по детерминированному URL:
///   https://github.com/{owner}/{repo}/releases/download/{tag}/{asset}
///
/// Octokit не используем: для публичных ассетов URL детерминирован,
/// а через API мы бы тратили rate-limit и усложняли логику.
/// Octokit пригодится позже — для приватных релизов или листинга ассетов.
/// </summary>
public sealed class GitHubDownloader : IArchiveDownloader
{
    private readonly HttpClient _http;

    public GitHubDownloader(HttpClient http)
    {
        _http = http;
    }

    public string SourceType => "github";

    public async Task<Stream> DownloadAsync(
        ArchiveSourceRef source, CancellationToken ct)
    {
        if (source is not GitHubSourceRef gh)
        {
            throw new ArgumentException(
                $"Expected GitHubSourceRef, got {source.GetType().Name}.",
                nameof(source));
        }

        var url = BuildUrl(gh);

        using var response = await _http.GetAsync(
            url, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"GitHub returned {(int)response.StatusCode} " +
                $"({response.ReasonPhrase}) for {url}");
        }

        // Буферизуем содержимое в память, чтобы вернуть независимый stream.
        // Для больших ассетов (гигабайты) это плохо, но в MVP — ок.
        // v0.2.0: стриминг сразу в файл через callback.
        var ms = new MemoryStream();
        await using (var networkStream = await response.Content.ReadAsStreamAsync(ct))
        {
            await networkStream.CopyToAsync(ms, ct);
        }

        ms.Position = 0;
        return ms;
    }

    private static string BuildUrl(GitHubSourceRef source)
    {
        // repo может быть "owner/repo" или "owner/repo.git" — нормализуем.
        var repo = source.Repo.TrimEnd('/');
        if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            repo = repo[..^4];

        // Tag и asset могут содержать спецсимволы. URL-escape.
        var tag = Uri.EscapeDataString(source.Tag);
        var asset = Uri.EscapeDataString(source.Asset);

        return $"https://github.com/{repo}/releases/download/{tag}/{asset}";
    }
}
