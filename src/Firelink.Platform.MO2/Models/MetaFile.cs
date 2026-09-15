namespace Firelink.Platform.MO2.Models;

public sealed class MetaFile
{
    public required string GameName { get; init; }
    public required int ModId { get; init; }
    public required int FileId { get; init; }
}
