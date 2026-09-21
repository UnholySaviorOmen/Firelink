using Firelink.Core.Models.Manifest.Directives;

namespace Firelink.Core.Models.Pack;

/// <summary>
/// Результат MatchExtensionsStep / MatchExtrasStep.
///
/// Отличие от MatchResult:
///   - MatchResult:      ключи — имена модов (из modlist.txt);
///                       Unmatched — UnmatchedFile (ModName + RelativePath).
///   - MatchEntriesResult: ключи — entryName (путь из config, например
///                       "tools/BethINI");
///                       Unmatched — UnmatchedEntry (EntryName + RelativePath).
///
/// Разные семантики — разные типы.
/// </summary>
public sealed class MatchEntriesResult
{
    public required IReadOnlyDictionary<string, IReadOnlyList<Directive>> Directives { get; init; }
    public required IReadOnlyList<UnmatchedEntry> Unmatched { get; init; }
}
