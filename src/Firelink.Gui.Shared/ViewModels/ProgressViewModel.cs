using CommunityToolkit.Mvvm.ComponentModel;

namespace Firelink.Gui.Shared.ViewModels;

public abstract partial class ProgressViewModel : ViewModel
{
    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private int _totalSteps;

    [ObservableProperty]
    private string _stepName = "";

    [ObservableProperty]
    private double _percent;

    [ObservableProperty]
    private bool _isBusy;

    public void Report(int step, int total, string name)
    {
        CurrentStep = step;
        TotalSteps = total;
        StepName = name;
        Percent = total > 0 ? (double)step / total * 100 : 0;
    }

    public void Reset()
    {
        CurrentStep = 0;
        TotalSteps = 0;
        StepName = "";
        Percent = 0;
        IsBusy = false;
    }
}
