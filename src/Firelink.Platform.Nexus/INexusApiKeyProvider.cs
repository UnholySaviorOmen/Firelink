namespace Firelink.Platform.Nexus;

/// <summary>
/// Источник Nexus API-ключа.
///
/// Абстракция нужна, чтобы NexusClient и NexusDownloader тестировались
/// без реального файла в %USERPROFILE%\.firelink\nexus.key.
/// </summary>
public interface INexusApiKeyProvider
{
    /// <summary>
    /// Возвращает API-ключ или null, если ключа нет / файл недоступен / файл пуст.
    /// Никогда не бросает исключение.
    /// </summary>
    string? TryGetApiKey();
}
