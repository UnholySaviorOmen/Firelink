using Avalonia.Threading;
using Firelink.Gui.Shared.Logging;

namespace Firelink.Gui.Services;

/// <summary>
/// Реализация IUiDispatcher поверх Dispatcher.UIThread.
///
/// Post — fire-and-forget: логи не блокируют worker-потоки pipeline.
/// Порядок сохраняется: Dispatcher гарантирует FIFO для Post
/// с одного и того же контекста.
/// </summary>
internal sealed class AvaloniaUiDispatcher : IUiDispatcher
{
    public void Post(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            // Уже на UI-потоке: вызываем синхронно, чтобы не плодить
            // лишние Post'ы (важно для тестов и для случая, когда
            // лог пришёл синхронно из UI).
            action();
            return;
        }

        Dispatcher.UIThread.Post(action);
    }
}
