using FluentAssertions;
using Firelink.Gui.Shared.Logging;
using Firelink.Gui.Shared.ViewModels;
using Microsoft.Extensions.Logging;

namespace Firelink.Gui.Shared.Tests;

public class LogVMTests
{
    [Fact]
    public void Entries_ProxiesSink()
    {
        var sink = new ObservableLogSink();
        var vm = new LogVM(sink);

        sink.Add(new LogEntry(DateTimeOffset.Now, LogLevel.Information, "x"));

        vm.Entries.Should().HaveCount(1);
        vm.Entries[0].Message.Should().Be("x");
    }

    [Fact]
    public void ClearCommand_EmptiesCollection()
    {
        var sink = new ObservableLogSink();
        var vm = new LogVM(sink);
        sink.Add(new LogEntry(DateTimeOffset.Now, LogLevel.Information, "x"));
        sink.Add(new LogEntry(DateTimeOffset.Now, LogLevel.Information, "y"));

        vm.ClearCommand.Execute(null);

        vm.Entries.Should().BeEmpty();
    }
}
