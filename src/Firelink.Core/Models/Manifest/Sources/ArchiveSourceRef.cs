using System.Text.Json.Serialization;

namespace Firelink.Core.Models.Manifest.Sources;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(NexusSourceRef), "nexus")]
[JsonDerivedType(typeof(MirrorSourceRef), "mirror")]
public abstract class ArchiveSourceRef { }
