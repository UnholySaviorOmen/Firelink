namespace Firelink.Core.Models.Manifest.Sources;

public sealed class GitHubSourceRef : ArchiveSourceRef
{
    public required string Repo { get; init; }
    public required string Tag { get; init; }
    public required string Asset { get; init; }
}