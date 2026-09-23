using Microsoft.Extensions.Logging;

namespace Firelink.Platform.Nexus;

/// <summary>
/// Читает Nexus API-ключ из %USERPROFILE%\.firelink\nexus.key.
///
/// Формат файла: одна строка с ключом, опционально с BOM, опционально
/// с trailing \r\n. Пустые строки и пробелы вокруг ключа — обрезаются.
///
/// Никаких исключений наружу: если файла нет, путь недоступен, файл пуст —
/// возвращается null. Это сознательно: absence ключа — валидное состояние,
/// а не ошибка (пользователь может не иметь Nexus-аккаунта).
///
/// v0.2.0: DPAPI-шифрование, SQLite-хранилище.
/// </summary>
public sealed class NexusApiKeyProvider : INexusApiKeyProvider
{
    private const string FirelinkDirName = ".firelink";
    private const string KeyFileName = "nexus.key";

    private readonly string _keyFilePath;
    private readonly ILogger<NexusApiKeyProvider> _logger;

    public NexusApiKeyProvider(ILogger<NexusApiKeyProvider> logger)
        : this(logger, DefaultKeyFilePath())
    {
    }

    /// <summary>
    /// Для тестов: явный путь к файлу ключа.
    /// </summary>
    internal NexusApiKeyProvider(ILogger<NexusApiKeyProvider> logger, string keyFilePath)
    {
        _logger = logger;
        _keyFilePath = keyFilePath;
    }

    /// <summary>Путь к файлу ключа. Для тестов и диагностики.</summary>
    public string KeyFilePath => _keyFilePath;

    public string? TryGetApiKey()
    {
        try
        {
            if (!File.Exists(_keyFilePath))
            {
                _logger.LogDebug(
                    "Nexus API key not found at {Path}", _keyFilePath);
                return null;
            }

            var raw = File.ReadAllText(_keyFilePath);

            // Убираем BOM, если он есть (File.ReadAllText сохраняет его в строке).
            if (raw.Length > 0 && raw[0] == '\uFEFF')
                raw = raw[1..];

            var trimmed = raw.Trim();
            if (trimmed.Length == 0)
            {
                _logger.LogWarning(
                    "Nexus API key file is empty: {Path}", _keyFilePath);
                return null;
            }

            // Ключ — одна строка. Если в файле что-то ещё (случайный текст,
            // вторая строка), берём первую непустую.
            var firstLine = trimmed
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(firstLine))
            {
                _logger.LogWarning(
                    "Nexus API key file has no usable line: {Path}", _keyFilePath);
                return null;
            }

            return firstLine.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to read Nexus API key from {Path}", _keyFilePath);
            return null;
        }
    }

    private static string DefaultKeyFilePath()
    {
        var home = Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile);

        return Path.Combine(home, FirelinkDirName, KeyFileName);
    }
}
