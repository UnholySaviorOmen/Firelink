using Firelink.Gui.Shared.Logging;

namespace Firelink.Gui.Shared.Tests.Fakes;

/// <summary>
/// Тестовый IUiDispatcher: выполняет action синхронно.
/// Никакого UI-потока нет — но контракт (Post вызывает action) соблюдён.
///
/// Используется в тестах, где ObservableLogSink идёт через DI
/// (например, MainWindowVMTests): без этого DI не соберёт граф.
/// </summary>
public sealed class FakeUiDispatcher : IUiDispatcher
{
    public void Post(Action action) => action();
}
