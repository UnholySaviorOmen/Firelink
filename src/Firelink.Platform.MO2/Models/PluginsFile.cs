namespace Firelink.Platform.MO2.Models;

public sealed class PluginsFile
{
    public required IReadOnlyList<PluginEntry> Entries { get; init; }
}
