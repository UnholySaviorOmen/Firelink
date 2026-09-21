namespace Firelink.Core.Progress;

/// <summary>
/// Прогресс одного шага pipeline.
///
/// StepIndex — 1-based, для отображения «шаг 3 из 12».
/// TotalSteps — общее число шагов в pipeline.
/// StepName — короткое стабильное имя шага, для логов и UI.
///
/// Общий тип для packer-а и installer-а. Не привязан ни к одной
/// из библиотек — поэтому живёт в Core.
/// </summary>
public readonly record struct StepProgress(
    int StepIndex,
    int TotalSteps,
    string StepName);
