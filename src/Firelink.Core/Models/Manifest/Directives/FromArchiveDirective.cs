using Firelink.Core.Models.Hashing;

namespace Firelink.Core.Models.Manifest.Directives;

public sealed class FromArchiveDirective : Directive
{
    public required string Archive { get; init; }
    public required string Source { get; init; }
    public required XxHash64Value Hash { get; init; }
    public required long Size { get; init; }
}