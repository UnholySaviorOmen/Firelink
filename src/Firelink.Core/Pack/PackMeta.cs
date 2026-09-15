namespace Firelink.Core.Models.Pack;

public sealed record PackMeta
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Author { get; init; }
    public required string Game { get; init; }
    public required string GameVersion { get; init; }
}
