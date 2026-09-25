namespace Firelink.Gui.Shared.Services;

/// <summary>
/// Абстракция запуска процесса по пути к файлу.
/// Живёт в Shared (без Avalonia), реализуется в Firelink.Gui
/// через Process.Start с UseShellExecute = true.
///
/// Используется InstalledPackVM для запуска ModOrganizer.exe.
/// В тестах подменяется fake-ом — Process.Start не вызывается.
/// </summary>
public interface IProcessLauncher
{
    /// <summary>
    /// Открыть файл средствами ОС (ShellExecute).
    /// Бросает исключение, если файл не найден или запуск невозможен.
    /// </summary>
    void OpenFile(string path);
}
