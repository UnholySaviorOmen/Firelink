using System.Text.Json;
using System.Text.Json.Serialization;

namespace Firelink.Core.Models.Manifest;

public static class ManifestJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(ModlistManifest manifest)
        => JsonSerializer.Serialize(manifest, Options);

    public static ModlistManifest Deserialize(string json)
        => JsonSerializer.Deserialize<ModlistManifest>(json, Options)
           ?? throw new JsonException("Manifest is null");

    public static async Task<ModlistManifest> LoadAsync(string path, CancellationToken ct)
    {
        await using var fs = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<ModlistManifest>(fs, Options, ct)
               ?? throw new JsonException("Manifest is null");
    }

    public static async Task SaveAsync(string path, ModlistManifest manifest, CancellationToken ct)
    {
        await using var fs = File.Create(path);
        await JsonSerializer.SerializeAsync(fs, manifest, Options, ct);
    }
}