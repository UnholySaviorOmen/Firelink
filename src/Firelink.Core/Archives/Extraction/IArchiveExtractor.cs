namespace Firelink.Core.Archives.Extraction;

/// <summary>
/// Распаковка архивов.
/// </summary>
public interface IArchiveExtractor
{
    /// <summary>
    /// Поддерживается ли этот формат.
    /// </summary>
    bool CanExtract(string archivePath);

    /// <summary>
    /// Распаковать архив в указанную папку.
    /// Папка должна существовать.
    /// Возвращает список извлечённых файлов (относительные пути внутри архива).
    /// </summary>
    Task<IReadOnlyList<string>> ExtractAsync(
        string archivePath,
        string destinationDirectory,
        CancellationToken ct);
}
