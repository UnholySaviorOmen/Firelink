using Firelink.Core.Models.Hashing;

namespace Firelink.Core.Models.Manifest.Sources;

public sealed class MirrorSourceRef : ArchiveSourceRef
{
    public required string Url { get; init; }
    public required XxHash64Value Hash { get; init; }
}