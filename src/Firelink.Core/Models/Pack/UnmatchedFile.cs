namespace Firelink.Core.Models.Pack;

/// <summary>
/// Файл мода, который не удалось восстановить из архивов.
/// Пакер выгружает такие файлы в __Firelink_Output с сохранением структуры,
/// чтобы автор мог увидеть свои правки и решить, делать ли из них патч.
/// </summary>
public readonly record struct UnmatchedFile(
    string ModName,
    string RelativePath,
    long Size);
