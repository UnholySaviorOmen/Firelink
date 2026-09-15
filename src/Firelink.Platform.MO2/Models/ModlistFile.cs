namespace Firelink.Platform.MO2.Models;

/// <summary>
/// Содержимое modlist.txt: плоский список записей в порядке файла.
/// </summary>
public sealed class ModlistFile
{
    public required IReadOnlyList<ModlistEntry> Entries { get; init; }

    /// <summary>Все записи без сепараторов (Enabled не важен).</summary>
    public IEnumerable<ModlistEntry> AllMods => Entries;
}
