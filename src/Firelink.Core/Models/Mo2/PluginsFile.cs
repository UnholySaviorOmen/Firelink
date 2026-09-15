namespace Firelink.Core.Models.Mo2;

public sealed class PluginsFile
{
    public required IReadOnlyList<PluginEntry> Entries { get; init; }
}
