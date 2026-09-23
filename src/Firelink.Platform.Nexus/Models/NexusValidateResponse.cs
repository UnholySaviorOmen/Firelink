using System.Text.Json.Serialization;

namespace Firelink.Platform.Nexus.Models;

/// <summary>
/// Ответ /v1/users/validate.json.
/// Берём только то, что нужно: is_premium (для проверки перед скачиванием)
/// и name (для логов).
/// </summary>
internal sealed class NexusValidateResponse
{
    [JsonPropertyName("is_premium")]
    public bool IsPremium { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
