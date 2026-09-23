using FluentAssertions;
using Firelink.Gui.Shared.Logging;
using Firelink.Gui.Shared.Tests.Fakes;
using Microsoft.Extensions.Logging;

namespace Firelink.Gui.Shared.Tests;

// Существующий класс ObservableLogSinkTests — оставить.
// Ниже — новые тесты, добавить в тот же файл.

public class ObservableLogSinkDispatcherTests
{
    private static LogEntry Entry(string msg, LogLevel level = LogLevel.Information)
        => new(DateTimeOffset.Now, level, msg);

    [Fact]
    public void WithDispatcher_Add_InvokesDispatcher()
    {
        var dispatcher = new FakeUiDispatcher();
        var sink = new ObservableLogSink(dispatcher);

        sink.Add(Entry("a"));

        // FakeUiDispatcher выполняет action синхронно,
        // поэтому запись уже в коллекции.
        sink.Entries.Should().HaveCount(1);
        sink.Entries[0].Message.Should().Be("a");
    }

    [Fact]
    public void WithDispatcher_Clear_InvokesDispatcher()
    {
        var dispatcher = new FakeUiDispatcher();
        var sink = new ObservableLogSink(dispatcher);

        sink.Add(Entry("a"));
        sink.Add(Entry("b"));
        sink.Clear();

        sink.Entries.Should().BeEmpty();
    }

    [Fact]
    public void WithoutDispatcher_Add_Synchronous()
    {
        var sink = new ObservableLogSink();
        sink.Add(Entry("a"));

        sink.Entries.Should().HaveCount(1);
    }

    [Fact]
    public void WithDispatcher_Trim_StillWorks()
    {
        var dispatcher = new FakeUiDispatcher();
        var sink = new ObservableLogSink(dispatcher);

        for (int i = 0; i < 250; i++)
            sink.Add(Entry($"msg-{i}"));

        sink.Entries.Should().HaveCount(200);
        sink.Entries[0].Message.Should().Be("msg-50");
        sink.Entries[^1].Message.Should().Be("msg-249");
    }

    /// <summary>
    /// Симулирует «настоящий» UI-диспетчер: action не выполняется
    /// синхронно, а копится в очередь. Проверяем, что до Flush
    /// коллекция не тронута — то есть Add реально маршалится.
    /// </summary>
    [Fact]
    public void WithDeferredDispatcher_Add_DoesNotMutateUntilFlush()
    {
        var dispatcher = new DeferredUiDispatcher();
        var sink = new ObservableLogSink(dispatcher);

        sink.Add(Entry("a"));
        sink.Add(Entry("b"));

        // До flush — пусто.
        sink.Entries.Should().BeEmpty();

        dispatcher.Flush();

        sink.Entries.Should().HaveCount(2);
        sink.Entries[0].Message.Should().Be("a");
        sink.Entries[1].Message.Should().Be("b");
    }

    private sealed class DeferredUiDispatcher : IUiDispatcher
    {
        private readonly List<Action> _queue = new();

        public void Post(Action action) => _queue.Add(action);

        public void Flush()
        {
            var copy = _queue.ToList();
            _queue.Clear();
            foreach (var a in copy) a();
        }
    }
}
