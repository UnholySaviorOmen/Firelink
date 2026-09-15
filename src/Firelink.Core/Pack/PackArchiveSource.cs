using Firelink.Core.Models.Manifest.Sources;

namespace Firelink.Core.Models.Pack;

/// <summary>
/// Явное указание источников для архива, у которого нет .meta-файла.
/// </summary>
public sealed record PackArchiveSource
{
    /// <summary>Имя файла в downloads/ (например, "SomeMod.7z").</summary>
    public required string Archive { get; init; }

    /// <summary>Источники в порядке приоритета. Непустой.</summary>
    public required IReadOnlyList<ArchiveSourceRef> Sources { get; init; }
}
