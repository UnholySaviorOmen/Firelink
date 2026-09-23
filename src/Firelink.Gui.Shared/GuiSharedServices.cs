using Firelink.Gui.Shared.Logging;
using Firelink.Gui.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Firelink.Gui.Shared;

public static class GuiSharedServices
{
    public static IServiceCollection AddGuiShared(this IServiceCollection services)
    {
        services.AddSingleton<ObservableLogSink>();

        services.AddSingleton<ILoggerProvider>(sp =>
            new ObservableLoggerProvider(
                sp.GetRequiredService<ObservableLogSink>(),
                minLevel: LogLevel.Information));

        services.AddSingleton<LogVM>();

        services.AddSingleton<HomeVM>();

        // MainWindowVM регистрируется в клиенте (Firelink.Gui), потому что
        // зависит от IScreenFactory, реализация которого живёт в exe-проекте.

        return services;
    }
}
