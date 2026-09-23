namespace Firelink.Platform.Nexus;

/// <summary>
/// Никогда не возвращает ключ. Полезен как fallback для DI-регистрации,
/// а также в тестах, где проверяется поведение «ключа нет».
/// </summary>
public sealed class NullNexusApiKeyProvider : INexusApiKeyProvider
{
    public string? TryGetApiKey() => null;
}
