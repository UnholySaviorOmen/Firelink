using FluentAssertions;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.ViewModels;

namespace Firelink.Gui.Shared.Tests;

public class NavigationVMTests
{
    private static (NavigationVM vm, SettingsVM settings, List<ScreenType> navigated)
        Make()
    {
        var settings = new SettingsVM();
        var navigated = new List<ScreenType>();
        var vm = new NavigationVM(s => navigated.Add(s), settings);
        return (vm, settings, navigated);
    }

    // ------------------------------------------------------------------
    //  DevMode = false (default)
    // ------------------------------------------------------------------

    [Fact]
    public void Default_ShowsOnlyHomeAndSettings()
    {
        var (vm, _, _) = Make();

        vm.Items.Should().HaveCount(2);
        vm.Items.Select(i => i.Screen).Should().Equal(
            ScreenType.Home,
            ScreenType.Settings);
    }

    [Fact]
    public void Default_NoNavigationItemsForInstallPackVerifyLogs()
    {
        var (vm, _, _) = Make();

        vm.Items.Should().NotContain(i => i.Screen == ScreenType.Install);
        vm.Items.Should().NotContain(i => i.Screen == ScreenType.Pack);
        vm.Items.Should().NotContain(i => i.Screen == ScreenType.Verify);
        vm.Items.Should().NotContain(i => i.Screen == ScreenType.Logs);
    }

    // ------------------------------------------------------------------
    //  DevMode = true
    // ------------------------------------------------------------------

    [Fact]
    public void DevModeOn_ShowsAllSixItems()
    {
        var (vm, settings, _) = Make();

        settings.IsDevMode = true;

        vm.Items.Should().HaveCount(6);
        vm.Items.Select(i => i.Screen).Should().Equal(
            ScreenType.Home,
            ScreenType.Install,
            ScreenType.Pack,
            ScreenType.Verify,
            ScreenType.Logs,
            ScreenType.Settings);
    }

    [Fact]
    public void DevModeOnThenOff_BackToTwoItems()
    {
        var (vm, settings, _) = Make();

        settings.IsDevMode = true;
        vm.Items.Should().HaveCount(6);

        settings.IsDevMode = false;
        vm.Items.Should().HaveCount(2);
        vm.Items.Select(i => i.Screen).Should().Equal(
            ScreenType.Home,
            ScreenType.Settings);
    }

    // ------------------------------------------------------------------
    //  SelectedItem сохраняется при перестройке
    // ------------------------------------------------------------------

    [Fact]
    public void DevModeOn_KeepsSelectedHome()
    {
        var (vm, settings, _) = Make();

        vm.SelectScreen(ScreenType.Home);
        settings.IsDevMode = true;

        vm.SelectedItem!.Screen.Should().Be(ScreenType.Home);
    }

    [Fact]
    public void DevModeOff_FromInstall_SwitchesToHome_AndNavigates()
    {
        var (vm, settings, navigated) = Make();

        settings.IsDevMode = true;
        vm.SelectScreen(ScreenType.Install);
        navigated.Clear();

        settings.IsDevMode = false;

        // SelectedItem переключился на Home.
        vm.SelectedItem!.Screen.Should().Be(ScreenType.Home);

        // И MainWindowVM уведомлён.
        navigated.Should().ContainSingle().Which.Should().Be(ScreenType.Home);
    }

    [Fact]
    public void DevModeOff_FromSettings_KeepsSettings()
    {
        var (vm, settings, navigated) = Make();

        settings.IsDevMode = true;
        vm.SelectScreen(ScreenType.Settings);
        navigated.Clear();

        settings.IsDevMode = false;

        vm.SelectedItem!.Screen.Should().Be(ScreenType.Settings);

        // Settings виден и после выключения DevMode — навигации не было.
        navigated.Should().BeEmpty();
    }

    [Fact]
    public void DevModeOff_FromPack_SwitchesToHome()
    {
        var (vm, settings, navigated) = Make();

        settings.IsDevMode = true;
        vm.SelectScreen(ScreenType.Pack);
        navigated.Clear();

        settings.IsDevMode = false;

        vm.SelectedItem!.Screen.Should().Be(ScreenType.Home);
        navigated.Should().ContainSingle().Which.Should().Be(ScreenType.Home);
    }

    // ------------------------------------------------------------------
    //  SelectScreen
    // ------------------------------------------------------------------

    [Fact]
    public void SelectScreen_KnownScreen_SetsSelectedItem_WithoutCallback()
    {
        var (vm, settings, navigated) = Make();
        settings.IsDevMode = true;

        // Включение DevMode с невыбранным SelectedItem уведомляет
        // MainWindowVM о переходе на Home (см. RebuildItems).
        // Для проверки SelectScreen это шум — обнуляем.
        navigated.Clear();

        vm.SelectScreen(ScreenType.Pack);

        vm.SelectedItem!.Screen.Should().Be(ScreenType.Pack);
        navigated.Should().BeEmpty("SelectScreen должен подавлять callback");
    }

    [Fact]
    public void SelectScreen_HiddenScreen_DoesNothing()
    {
        var (vm, _, navigated) = Make();
        // DevMode = false, Install скрыт.

        vm.SelectScreen(ScreenType.Install);

        vm.SelectedItem.Should().BeNull();
        navigated.Should().BeEmpty();
    }

    // ------------------------------------------------------------------
    //  Клик пользователя
    // ------------------------------------------------------------------

    [Fact]
    public void SelectedItemChange_InvokesCallback()
    {
        var (vm, _, navigated) = Make();
        var settingsItem = vm.Items.First(i => i.Screen == ScreenType.Settings);

        vm.SelectedItem = settingsItem;

        navigated.Should().ContainSingle().Which.Should().Be(ScreenType.Settings);
    }

    [Fact]
    public void SelectedItemNull_DoesNotInvokeCallback()
    {
        var (vm, _, navigated) = Make();

        vm.SelectedItem = null;

        navigated.Should().BeEmpty();
    }
}
