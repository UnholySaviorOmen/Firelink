namespace Firelink.Gui.Shared.ViewModels;

/// <summary>
/// VM экрана Logs. По сути — обёртка над LogVM (тот же ObservableLogSink,
/// зарегистрированный как Singleton). Логи общие между всеми экранами.
/// </summary>
public sealed class LogsVM : ViewModel
{
    public LogVM Log { get; }

    public LogsVM(LogVM log)
    {
        Log = log;
    }
}
