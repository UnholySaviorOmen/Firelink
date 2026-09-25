using FluentAssertions;
using Firelink.Gui.Shared.Logging;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.Services;
using Firelink.Gui.Shared.Tests.Fakes;
using Firelink.Gui.Shared.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firelink.Gui.Shared.Tests;

public class MainWindowVMTests
{
    private static ServiceProvider BuildProvider(
        FakeInstalledPackScanner? scanner = null)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IUiDispatcher, FakeUiDispatcher>();
        services.AddGuiShared();

        // Заменяем инфраструктурные сервисы на fake'и: HomeVM их требует.
        services.RemoveAll<IInstalledPackScanner>();
        services.AddSingleton<IInstalledPackScanner>(
            scanner ?? new FakeInstalledPackScanner());

        services.RemoveAll<IFilePickerService>();
        services.AddSingleton<IFilePickerService>(new FakeFilePickerService());

        services.RemoveAll<IProcessLauncher>();
        services.AddSingleton<IProcessLauncher>(new FakeProcessLauncher());

        services.AddSingleton<IScreenFactory>(sp => new FakeScreenFactory(sp));
        services.AddSingleton<MainWindowVM>();

        return services.BuildServiceProvider();
    }

    // ------------------------------------------------------------------
    //  Базовое состояние
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_ActivePane_IsHome()
    {
        using var sp = BuildProvider();
        var vm = sp.GetRequiredService<MainWindowVM>();

        vm.ActivePane.Should().BeOfType<HomeVM>();
        vm.Navigation.SelectedItem!.Screen.Should().Be(ScreenType.Home);
    }

    [Fact]
    public void Constructor_HomeRefreshCalled()
    {
        var scanner = new FakeInstalledPackScanner();
        scanner.Packs.Add(FakeInstalledPackScanner.MakePack(name: "Initial"));

        using var sp = BuildProvider(scanner);
        var vm = sp.GetRequiredService<MainWindowVM>();

        vm.Home.Items.Should().HaveCount(1);
        vm.Home.Items[0].Name.Should().Be("Initial");
    }

    // ------------------------------------------------------------------
    //  Навигация
    // ------------------------------------------------------------------

    [Fact]
    public void NavigateTo_Home_ReturnsSameInstance()
    {
        using var sp = BuildProvider();
        var vm = sp.GetRequiredService<MainWindowVM>();
        var home = vm.Home;

        vm.NavigateTo(ScreenType.Home);

        vm.ActivePane.Should().BeSameAs(home);
    }

    [Fact]
    public void NavigateTo_Home_RefreshesItems()
    {
        var scanner = new FakeInstalledPackScanner();
        using var sp = BuildProvider(scanner);
        var vm = sp.GetRequiredService<MainWindowVM>();

        vm.Home.Items.Should().BeEmpty();

        scanner.Packs.Add(FakeInstalledPackScanner.MakePack(name: "New Pack"));

        vm.NavigateTo(ScreenType.Home);

        vm.Home.Items.Should().HaveCount(1);
        vm.Home.Items[0].Name.Should().Be("New Pack");
    }

    [Fact]
    public void NavigateTo_SameScreenTwice_ReturnsSameInstance()
    {
        using var sp = BuildProvider();
        var vm = sp.GetRequiredService<MainWindowVM>();

        vm.NavigateTo(ScreenType.Home);
        var first = vm.ActivePane;
        vm.NavigateTo(ScreenType.Home);
        var second = vm.ActivePane;

        second.Should().BeSameAs(first);
    }

    // ------------------------------------------------------------------
    //  SettingsVM / DevMode
    // ------------------------------------------------------------------

    [Fact]
    public void Navigation_ExposesSettingsVM_SameInstanceAsDI()
    {
        using var sp = BuildProvider();
        var vm = sp.GetRequiredService<MainWindowVM>();
        var settings = sp.GetRequiredService<SettingsVM>();

        settings.IsDevMode = true;

        vm.Navigation.Items.Should().HaveCount(6);
    }

    // ------------------------------------------------------------------
    //  InstallRequested (Home → Install)
    // ------------------------------------------------------------------

    [Fact]
    public void InstallRequested_FromHome_DoesNotThrow()
    {
        var scanner = new FakeInstalledPackScanner();
        scanner.Packs.Add(FakeInstalledPackScanner.MakePack());

        using var sp = BuildProvider(scanner);
        var vm = sp.GetRequiredService<MainWindowVM>();

        var act = () => vm.Home.Items[0].InstallCommand.Execute(null);
        act.Should().NotThrow();

        // В этом окружении IInstallTarget не реализуется ни одной VM,
        // так что вызов просто пройдёт вхолостую.
        vm.ActivePane.Should().BeOfType<HomeVM>();
    }
}
