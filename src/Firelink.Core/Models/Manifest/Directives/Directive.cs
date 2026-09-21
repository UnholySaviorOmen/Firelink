using System.Text.Json.Serialization;

namespace Firelink.Core.Models.Manifest.Directives;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FromArchiveDirective), "FromArchive")]
[JsonDerivedType(typeof(CreateDirectoryDirective), "CreateDirectory")]
[JsonDerivedType(typeof(DeleteDirective), "Delete")]
public abstract class Directive
{
    public required string Destination { get; init; }
}
