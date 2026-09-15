using System.Globalization;

namespace Firelink.Core.Identity;

/// <summary>
/// Генератор канонических id архивов.
/// Формат:
///   nexus_{game_domain}_{modId}_{fileId}   — для Nexus-модов (есть .meta)
///   local_{slug}                            — для архивов без .meta
///   github_{owner}_{repo}_{tag}_{assetSlug} — для GitHub-релизов
/// </summary>
public static class ArchiveId
{
    public const string NexusPrefix = "nexus_";
    public const string LocalPrefix = "local_";
    public const string GitHubPrefix = "github_";

    /// <summary>
    /// Канонический id для Nexus-архива.
    /// </summary>
    public static string FromNexus(string gameDomain, int modId, int fileId)
    {
        if (string.IsNullOrWhiteSpace(gameDomain))
            throw new ArgumentException("Game domain must be non-empty.", nameof(gameDomain));
        if (modId <= 0)
            throw new ArgumentOutOfRangeException(nameof(modId), "modId must be positive.");
        if (fileId <= 0)
            throw new ArgumentOutOfRangeException(nameof(fileId), "fileId must be positive.");

        var game = gameDomain.Trim().ToLowerInvariant();

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{NexusPrefix}{game}_{modId}_{fileId}");
    }

    /// <summary>
    /// Id для архива без .meta. Использует slug от имени файла (без расширения).
    /// </summary>
    public static string FromLocal(string fileName)
    {
        var slug = Slug.FromFileName(fileName);
        return LocalPrefix + slug;
    }

    /// <summary>
    /// Id для архива из GitHub-релиза.
    /// </summary>
    public static string FromGitHub(string owner, string repo, string tag, string assetName)
    {
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Owner must be non-empty.", nameof(owner));
        if (string.IsNullOrWhiteSpace(repo))
            throw new ArgumentException("Repo must be non-empty.", nameof(repo));
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag must be non-empty.", nameof(tag));
        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentException("Asset name must be non-empty.", nameof(assetName));

        var ownerSlug = Slug.From(owner);
        var repoSlug = Slug.From(repo);
        var tagSlug = Slug.From(tag);
        var assetSlug = Slug.FromFileName(assetName);

        return $"{GitHubPrefix}{ownerSlug}_{repoSlug}_{tagSlug}_{assetSlug}";
    }
}
