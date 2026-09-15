namespace Firelink.Core.Models.Mo2;

public sealed class ModlistFile
{
    public required IReadOnlyList<ModlistEntry> Entries { get; init; }
    public IEnumerable<ModlistEntry> AllMods => Entries;
}
