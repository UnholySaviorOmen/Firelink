using Firelink.Gui.Install.Services;
using Firelink.Gui.Install.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Firelink.Gui.Install;

public static class GuiInstallServices
{
    public static IServiceCollection AddGuiInstall(this IServiceCollection services)
    {
        services.AddSingleton<IInstallRunner, InstallRunner>();
        services.AddSingleton<InstallVM>();
        return services;
    }
}
