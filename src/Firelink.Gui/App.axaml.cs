using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Firelink.Gui.Install;
using Firelink.Gui.Pack;
using Firelink.Gui.Services;
using Firelink.Gui.Shared;
using Firelink.Gui.Shared.Logging;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.Services;
using Firelink.Gui.Shared.ViewModels;
using Firelink.Gui.Verify;
using Firelink.Install;
using Firelink.Pack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Firelink.Gui;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        DataTemplates.Add(new ViewLocator());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Services = BuildServices();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowVM>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Pipeline-сервисы.
        services.AddFirelinkInstall();
        services.AddFirelinkPack();

        // UI-инфраструктура exe-проекта.
        services.AddSingleton<IUiDispatcher, AvaloniaUiDispatcher>();
        services.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
        services.AddSingleton<IScreenFactory, ScreenFactory>();

        // GUI-модули.
        services.AddGuiShared();
        services.AddGuiInstall();
        services.AddGuiPack();
        services.AddGuiVerify();

        // Базовые экраны (Shared-уровень).
        services.AddSingleton<SettingsVM>();

        services.AddSingleton<MainWindowVM>();

        return services.BuildServiceProvider();
    }
}
