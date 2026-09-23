namespace Firelink.Gui.Shared.Logging;

/// <summary>
/// Абстракция над UI-диспетчером.
///
/// Зачем: ObservableLogSink принимает логи из worker-потоков pipeline
/// (Parallel.ForEach). ObservableCollection&lt;T&gt; не потокобезопасен:
/// мутация из worker-потока параллельно с UI-биндингом (ItemsControl
/// через CollectionChanged) — гонка и падение процесса вне try/catch VM.
///
/// Решение: любая мутация коллекции — через Post на UI-поток.
/// Firelink.Gui.Shared не знает про Avalonia; IUiDispatcher —
/// тонкая абстракция. Реализация (AvaloniaUiDispatcher) — в Firelink.Gui.
///
/// Post — fire-and-forget. Возврата значения нет: логи не блокируют
/// worker-потоки.
/// </summary>
public interface IUiDispatcher
{
    void Post(Action action);
}
