using FluentAssertions;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.ViewModels;

namespace Firelink.Gui.Shared.Tests;

public class NavigationVMTests
{
    [Fact]
    public void Constructor_PopulatesNavigationItems()
    {
        var vm = new NavigationVM(_ => { });

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
    public void SelectedItem_InvokesCallback()
    {
        ScreenType? captured = null;
        var vm = new NavigationVM(s => captured = s);

        vm.SelectedItem = vm.Items[1]; // Install

        captured.Should().Be(ScreenType.Install);
    }

    [Fact]
    public void SelectScreen_SetsSelectedItem_WithoutInvokingCallback()
    {
        ScreenType? captured = null;
        var vm = new NavigationVM(s => captured = s);

        vm.SelectScreen(ScreenType.Pack);

        vm.SelectedItem.Should().NotBeNull();
        vm.SelectedItem!.Screen.Should().Be(ScreenType.Pack);
        captured.Should().BeNull("SelectScreen должен подавлять callback");
    }

    [Fact]
    public void SelectScreen_UnknownScreen_DoesNothing()
    {
        var vm = new NavigationVM(_ => { });
        vm.SelectScreen((ScreenType)999);
        vm.SelectedItem.Should().BeNull();
    }

    [Fact]
    public void SelectedItem_Null_DoesNotInvokeCallback()
    {
        ScreenType? captured = null;
        var vm = new NavigationVM(s => captured = s);

        vm.SelectedItem = null;

        captured.Should().BeNull();
    }
}
