using FluentAssertions;
using Firelink.Gui.Shared.ViewModels;

namespace Firelink.Gui.Shared.Tests;

public class SettingsVMTests
{
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
}
