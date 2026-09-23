using FluentAssertions;
using Firelink.Gui.Shared.ViewModels;

namespace Firelink.Gui.Shared.Tests;

public class ProgressViewModelTests
{
    private sealed class TestProgressVM : ProgressViewModel { }

    [Fact]
    public void Report_SetsAllFields()
    {
        var vm = new TestProgressVM();
        vm.Report(3, 11, "SyncMods");

        vm.CurrentStep.Should().Be(3);
        vm.TotalSteps.Should().Be(11);
        vm.StepName.Should().Be("SyncMods");
        vm.Percent.Should().BeApproximately(300.0 / 11.0, 0.01);
    }

    [Fact]
    public void Report_TotalZero_PercentIsZero()
    {
        var vm = new TestProgressVM();
        vm.Report(0, 0, "");

        vm.Percent.Should().Be(0);
    }

    [Fact]
    public void Reset_ClearsAllFields()
    {
        var vm = new TestProgressVM();
        vm.Report(5, 10, "X");
        vm.IsBusy = true;
        vm.Reset();

        vm.CurrentStep.Should().Be(0);
        vm.TotalSteps.Should().Be(0);
        vm.StepName.Should().Be("");
        vm.Percent.Should().Be(0);
        vm.IsBusy.Should().BeFalse();
    }
}
