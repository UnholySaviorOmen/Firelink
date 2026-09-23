using Firelink.Gui.Shared.Logging;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.Tests.Fakes;
using Firelink.Gui.Shared.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Firelink.Gui.Shared.Tests;

public class MainWindowVMTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IUiDispatcher, FakeUiDispatcher>();
        services.AddGuiShared();

        services.AddSingleton<IScreenFactory>(sp => new FakeScreenFactory(sp));
        services.AddSingleton<MainWindowVM>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_ActivePane_IsHome()
    {
        using var sp = BuildProvider();
        var vm = sp.GetRequiredService<MainWindowVM>();

        vm.ActivePane.Should().BeOfType<HomeVM>();
        vm.Navigation.SelectedItem!.Screen.Should().Be(ScreenType.Home);
    }

    [Fact]
    public void NavigateTo_Home_ReturnsSameInstance()
    {
        using var sp = BuildProvider();
        var vm = sp.GetRequiredService<MainWindowVM>();
        var home = vm.Home;

        vm.NavigateTo(ScreenType.Install);
        vm.NavigateTo(ScreenType.Home);

        vm.ActivePane.Should().BeSameAs(home);
    }

    [Fact]
    public void NavigateTo_SameScreenTwice_ReturnsSameInstance()
    {
        using var sp = BuildProvider();
        var vm = sp.GetRequiredService<MainWindowVM>();

        vm.NavigateTo(ScreenType.Install);
        var first = vm.ActivePane;
        vm.NavigateTo(ScreenType.Install);
        var second = vm.ActivePane;

        second.Should().BeSameAs(first);
    }
}
