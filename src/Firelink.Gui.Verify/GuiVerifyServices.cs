using Firelink.Gui.Verify.Services;
using Firelink.Gui.Verify.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Firelink.Gui.Verify;

public static class GuiVerifyServices
{
    public static IServiceCollection AddGuiVerify(this IServiceCollection services)
    {
        services.AddSingleton<IVerifyRunner, VerifyRunner>();
        services.AddSingleton<VerifyVM>();
        return services;
    }
}
