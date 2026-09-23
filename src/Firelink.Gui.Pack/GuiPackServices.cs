using Firelink.Gui.Pack.Services;
using Firelink.Gui.Pack.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Firelink.Gui.Pack;

public static class GuiPackServices
{
    public static IServiceCollection AddGuiPack(this IServiceCollection services)
    {
        services.AddSingleton<IPackRunner, PackRunner>();
        services.AddSingleton<PackVM>();
        return services;
    }
}
