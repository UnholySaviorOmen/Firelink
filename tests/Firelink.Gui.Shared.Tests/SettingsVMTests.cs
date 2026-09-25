using FluentAssertions;
using Firelink.Gui.Shared.ViewModels;

namespace Firelink.Gui.Shared.Tests;

public class SettingsVMTests
{
    // ------------------------------------------------------------------
    //  About
    // ------------------------------------------------------------------

    [Fact]
    public void ProductName_IsFirelink()
    {
        var vm = new SettingsVM();
        vm.ProductName.Should().Be("Firelink");
    }

    [Fact]
    public void Version_IsNotEmpty()
    {
        var vm = new SettingsVM();
        vm.Version.Should().NotBeNullOrWhiteSpace();
        vm.Version.Should().NotBe("0.0.0");
    }

    [Fact]
    public void Copyright_Is2026Omen()
    {
        var vm = new SettingsVM();
        vm.Copyright.Should().Be("Copyright (C) 2026 omen");
    }

    [Fact]
    public void License_IsAgpl3OrLater()
    {
        var vm = new SettingsVM();
        vm.License.Should().Be("AGPL-3.0-or-later");
    }

    [Fact]
    public void Footer_ContainsAllParts()
    {
        var vm = new SettingsVM();
        vm.Footer.Should().Contain("Firelink");
        vm.Footer.Should().Contain(vm.Version);
        vm.Footer.Should().Contain("AGPL-3.0-or-later");
        vm.Footer.Should().Contain("Copyright (C) 2026 omen");
    }

    // ------------------------------------------------------------------
    //  DevMode
    // ------------------------------------------------------------------

    [Fact]
    public void IsDevMode_DefaultsToFalse()
    {
        var vm = new SettingsVM();
        vm.IsDevMode.Should().BeFalse();
    }

    [Fact]
    public void IsDevMode_CanBeToggled()
    {
        var vm = new SettingsVM();

        vm.IsDevMode = true;
        vm.IsDevMode.Should().BeTrue();

        vm.IsDevMode = false;
        vm.IsDevMode.Should().BeFalse();
    }

    [Fact]
    public void IsDevMode_RaisesPropertyChanged()
    {
        var vm = new SettingsVM();
        var changed = new List<string?>();

        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.IsDevMode = true;

        changed.Should().Contain(nameof(SettingsVM.IsDevMode));
    }
}
