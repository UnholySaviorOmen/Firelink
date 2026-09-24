using System;
using System.IO;
using Avalonia;

namespace Firelink.Gui;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            var logPath = Path.Combine(
                Path.GetTempPath(), "firelink-gui-crash.txt");

            try
            {
                File.WriteAllText(logPath, ex.ToString());
            }
            catch
            {
                // Если и это не получилось — печатаем в stderr.
                Console.Error.WriteLine(ex);
            }

            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
