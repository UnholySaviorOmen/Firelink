using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Directives;

namespace Firelink.Core.Models.Manifest;

public sealed class ModlistManifest
{
    public required string SchemaVersion { get; init; }
    public required string ManifestVersion { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required string CreatedBy { get; init; }

    public required ManifestMeta Meta { get; init; }
    public required ExecutionPolicy Execution { get; init; }
    public required Mo2Section Mo2 { get; init; }
    public required StockGameSection StockGame { get; init; }
    public required IReadOnlyList<ArchiveEntry> Archives { get; init; }
    public required IReadOnlyList<ModEntry> Mods { get; init; }
    public required IReadOnlyList<PluginEntry> Plugins { get; init; }
    public required IReadOnlyList<string> Loadorder { get; init; }
    public required IReadOnlyList<InlineFileEntry> InlineFiles { get; init; }
}

public sealed class ManifestMeta
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Author { get; init; }
    public required string Game { get; init; }
    public required string GameVersion { get; init; }
}

public sealed class ExecutionPolicy
{
    public string Directives { get; init; } = "sequential";
    public string OnConflict { get; init; } = "lastWins";
}

public sealed class Mo2Section
{
    public required string Version { get; init; }
    public required ArchiveEntry Archive { get; init; }
    public required IReadOnlyList<ExtensionEntry> Extensions { get; init; }
}

public sealed class ExtensionEntry
{
    public required string Name { get; init; }
    public required IReadOnlyList<Directive> Directives { get; init; }
}

public sealed class StockGameSection
{
    public required IReadOnlyList<ExtensionEntry> Extras { get; init; }
}

public sealed class ModEntry
{
    public required string Name { get; init; }
    public required bool Enabled { get; init; }
    public required int Order { get; init; }
    public required IReadOnlyList<Directive> Directives { get; init; }
}

public sealed class PluginEntry
{
    public required string Name { get; init; }
    public required bool Enabled { get; init; }
    public required int Order { get; init; }
}

public sealed class InlineFileEntry
{
    public required string Id { get; init; }
    public required XxHash64Value Hash { get; init; }
    public required long Size { get; init; }
    public required string Content { get; init; }
}