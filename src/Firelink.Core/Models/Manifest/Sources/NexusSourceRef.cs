namespace Firelink.Core.Models.Manifest.Sources;

public sealed class NexusSourceRef : ArchiveSourceRef
{
    public required int ModId { get; init; }
    public required int FileId { get; init; }
    public required string Game { get; init; }
}