namespace Firelink.Core.Models.Manifest.Directives;

public sealed class InlineFileDirective : Directive
{
    public required string InlineFileId { get; init; }
}