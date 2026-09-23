using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Firelink.Gui.Shared.Tests.Fakes;

/// <summary>
/// FakeScreenFactory для тестов MainWindowVM в Firelink.Gui.Shared.Tests.
///
/// В этом проекте нет ссылок на Firelink.Gui.Pack / Firelink.Gui.Verify
/// (они Avalonia-зависимые, а Gui.Shared.Tests — нет). Поэтому фабрика
/// умеет только Home; для остальных экранов возвращает Home.
///
/// Реальная маршрутизация Pack/Install/Verify тестируется в их
/// собственных тестовых проектах.
/// </summary>
public sealed class FakeScreenFactory : IScreenFactory
{
    private readonly IServiceProvider _sp;

    public FakeScreenFactory(IServiceProvider sp) => _sp = sp;

    public object Create(ScreenType screen) => screen switch
    {
        ScreenType.Home => _sp.GetRequiredService<HomeVM>(),
        _ => _sp.GetRequiredService<HomeVM>(),
    };
}
