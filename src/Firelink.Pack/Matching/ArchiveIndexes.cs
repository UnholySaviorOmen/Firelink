using Firelink.Core.Models.Hashing;

namespace Firelink.Pack.Matching;

/// <summary>
/// Три индекса, построенные ArchiveMatcher-ом.
/// Тот же набор, что в MatchStep, но вынесен в отдельный тип.
/// </summary>
internal sealed record ArchiveIndexes(
    Dictionary<(XxHash64Value, string), ArchiveMatcher.IndexEntry> ByHashPath,
    Dictionary<XxHash64Value, List<ArchiveMatcher.IndexEntry>> ByHash,
    Dictionary<string, Dictionary<XxHash64Value, string>> ByPath);
