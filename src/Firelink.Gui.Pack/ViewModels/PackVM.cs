using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firelink.Core;
using Firelink.Core.Progress;
using Firelink.Gui.Pack.Services;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.Services;
using Firelink.Gui.Shared.State;
using Firelink.Gui.Shared.ViewModels;
using Firelink.Gui.Shared.ViewModels.Controls;
using Firelink.Pack;
using Microsoft.Extensions.Logging;

namespace Firelink.Gui.Pack.ViewModels;

public sealed partial class PackVM : ProgressViewModel, INavigationAware
{
    private readonly IPackRunner _runner;
    private readonly ILogger<PackVM> _logger;
    private Action? _navigateHome;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private PackState _state = PackState.Configuration;

    [ObservableProperty]
    private bool _isConfiguring = true;

    [ObservableProperty]
    private bool _isPacking;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private bool _isFailure;

    [ObservableProperty]
    private PackSummary? _summary;

    [ObservableProperty]
    private string? _errorMessage;

    public FilePickerVM ConfigPicker { get; }
    public LogVM Log { get; }

    public PackVM(
        IPackRunner runner,
        IFilePickerService picker,
        LogVM log,
        ILogger<PackVM> logger)
    {
        _runner = runner;
        _logger = logger;
        Log = log;

        ConfigPicker = new FilePickerVM(picker)
        {
            Placeholder = "Select firelink-pack.json",
            FilterHint = ".json",
            MustExist = true,
            Folder = false,
        };
        ConfigPicker.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FilePickerVM.IsValid))
                PackCommand.NotifyCanExecuteChanged();
        };
    }

    public void SetNavigateHome(Action navigateHome) => _navigateHome = navigateHome;

    [RelayCommand(CanExecute = nameof(CanPack))]
    private async Task PackAsync()
    {
        Log.Clear();

        State = PackState.Packing;
        UpdateVisibility();

        _cts = new CancellationTokenSource();

        var progress = new Progress<StepProgress>(p =>
            Report(p.StepIndex, p.TotalSteps, p.StepName));

        try
        {
            Summary = await _runner.RunAsync(
                ConfigPicker.Path!, progress, _cts.Token);

            State = PackState.Success;
        }
        catch (Exception ex) when (CancellationHelper.IsCancellation(ex))
        {
            _logger.LogInformation("Pack cancelled by user");
            ErrorMessage = "Cancelled.";
            State = PackState.Configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pack failed");
            ErrorMessage = ex.Message;
            State = PackState.Failure;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            UpdateVisibility();
        }
    }

    private bool CanPack()
        => State == PackState.Configuration && ConfigPicker.IsValid;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    private bool CanCancel() => State == PackState.Packing;

    [RelayCommand]
    private void Home() => _navigateHome?.Invoke();

    partial void OnStateChanged(PackState value)
    {
        UpdateVisibility();
        PackCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void UpdateVisibility()
    {
        IsConfiguring = State == PackState.Configuration;
        IsPacking = State == PackState.Packing;
        IsSuccess = State == PackState.Success;
        IsFailure = State == PackState.Failure;
    }
}
