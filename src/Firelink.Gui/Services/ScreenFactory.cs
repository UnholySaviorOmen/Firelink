using Firelink.Gui.Install.ViewModels;
using Firelink.Gui.Pack.ViewModels;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.ViewModels;
using Firelink.Gui.Verify.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Firelink.Gui.Services;

internal sealed class ScreenFactory : IScreenFactory
{
    private readonly IServiceProvider _sp;

    public ScreenFactory(IServiceProvider sp) => _sp = sp;

    public object Create(ScreenType screen) => screen switch
    {
        ScreenType.Home => _sp.GetRequiredService<HomeVM>(),
        ScreenType.Install => _sp.GetRequiredService<InstallVM>(),
        ScreenType.Pack => _sp.GetRequiredService<PackVM>(),
        ScreenType.Verify => _sp.GetRequiredService<VerifyVM>(),
        ScreenType.Logs => _sp.GetRequiredService<LogsVM>(),
        ScreenType.Settings => _sp.GetRequiredService<SettingsVM>(),
        _ => _sp.GetRequiredService<HomeVM>(),
    };
}
