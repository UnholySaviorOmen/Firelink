using Firelink.Core.Progress;
using Firelink.Pack;

namespace Firelink.Gui.Pack.Services;

/// <summary>
/// Обёртка над PackPipeline.
///
/// Зачем: PackVM тестируется без поднятия всего DI-графа pipeline.
/// В тестах IPackRunner заменяется fake-ом, который либо возвращает
/// готовый PackSummary, либо бросает исключение.
///
/// Реализация (PackRunner) — тонкая. Вся логика — в pipeline.
/// </summary>
public interface IPackRunner
{
    /// <summary>
    /// Запустить pack. Возвращает сводку результата.
    ///
    /// Бросает:
    ///   - OperationCanceledException (или AggregateException с cancellation)
    ///     при отмене через ct;
    ///   - любой exception из pipeline — при ошибке.
    /// </summary>
    Task<PackSummary> RunAsync(
        string configPath,
        IProgress<StepProgress> progress,
        CancellationToken ct);
}
