using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Directives;

namespace Firelink.Core.Models.Pack;

/// <summary>
/// Результат MatchStep.
/// </summary>
public sealed class MatchResult
{
    /// <summary>Директивы по каждому моду. Ключ — имя мода.</summary>
    public required IReadOnlyDictionary<string, IReadOnlyList<Directive>> ModDirectives { get; init; }

    /// <summary>Inline-файлы (которых нет в архивах).</summary>
    public required IReadOnlyList<InlineFileContent> InlineFiles { get; init; }

    /// <summary>Файлы, для которых не нашлось источника и они > лимита inline.</summary>
    public required IReadOnlyList<OrphanFile> Orphans { get; init; }
}

/// <summary>Inline-файл: содержимое + хеш + id.</summary>
public sealed class InlineFileContent
{
    public required string Id { get; init; }
    public required byte[] Content { get; init; }
    public required XxHash64Value Hash { get; init; }
}

/// <summary>Файл, для которого не нашлось источника (слишком большой для inline).</summary>
public sealed class OrphanFile
{
    public required string ModName { get; init; }
    public required string RelativePath { get; init; }
    public required long Size { get; init; }
}
