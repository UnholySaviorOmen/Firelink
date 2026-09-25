using System.Diagnostics;
using Firelink.Gui.Shared.Services;

namespace Firelink.Gui.Services;

/// <summary>
/// Реализация IProcessLauncher через Process.Start + ShellExecute.
///
/// UseShellExecute = true нужен, чтобы ОС сама разобралась, как
/// открыть файл — для .exe это запуск, для .txt — ассоциированная
/// программа. На Windows это работает из коробки.
/// </summary>
internal sealed class ShellProcessLauncher : IProcessLauncher
{
    public void OpenFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException(
                "Path must be non-empty.", nameof(path));

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
        });
    }
}
