using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firelink.Core;
using Firelink.Core.Progress;
using Firelink.Gui.Install.Services;
using Firelink.Gui.Shared.Navigation;
using Firelink.Gui.Shared.Services;
using Firelink.Gui.Shared.State;
using Firelink.Gui.Shared.ViewModels;
using Firelink.Gui.Shared.ViewModels.Controls;
using Firelink.Install;
using Microsoft.Extensions.Logging;

namespace Firelink.Gui.Install.ViewModels;

public sealed partial class InstallVM : ProgressViewModel, INavigationAware
{
    private readonly IInstallRunner _runner;
    private readonly ILogger<InstallVM> _logger;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private InstallState _state = InstallState.Configuration;

    [ObservableProperty]
    private bool _isConfiguring = true;

    [ObservableProperty]
    private bool _isInstalling;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private bool _isFailure;

    [ObservableProperty]
    private InstallSummary? _summary;

    [ObservableProperty]
    private string? _errorMessage;

    public FilePickerVM ModlistPicker { get; }
    public FilePickerVM TargetPicker { get; }
    public LogVM Log { get; }

    public InstallVM(
        IInstallRunner runner,
        IFilePickerService picker,
        LogVM log,
        ILogger<InstallVM> logger)
    {
        _runner = runner;
        _logger = logger;
        Log = log;

        ModlistPicker = new FilePickerVM(picker)
        {
            Placeholder = "Select modlist.json",
            FilterHint = ".json",
            MustExist = true,
            Folder = false,
        };
        ModlistPicker.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FilePickerVM.IsValid))
                InstallCommand.NotifyCanExecuteChanged();
        };

        TargetPicker = new FilePickerVM(picker)
        {
            Placeholder = "Target folder (optional)",
            MustExist = false,
            Folder = true,
        };
    }

    public void SetNavigateHome(Action navigateHome)
    {
        // Оставлено для совместимости с INavigationAware.
        // В UI кнопка Home заменена на Done (3.9.6).
    }

    [RelayCommand(CanExecute = nameof(CanInstall))]
    private async Task InstallAsync()
    {
        State = InstallState.Installing;
        UpdateVisibility();

        _cts = new CancellationTokenSource();

        var progress = new Progress<StepProgress>(p =>
            Report(p.StepIndex, p.TotalSteps, p.StepName));

        try
        {
            var target = string.IsNullOrWhiteSpace(TargetPicker.Path)
                ? null
                : TargetPicker.Path;

            Summary = await _runner.RunAsync(
                ModlistPicker.Path!, target, progress, _cts.Token);

            State = InstallState.Success;
        }
        catch (Exception ex) when (CancellationHelper.IsCancellation(ex))
        {
            _logger.LogInformation("Install cancelled by user");
            ErrorMessage = "Cancelled.";
            State = InstallState.Configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Install failed");
            ErrorMessage = ex.Message;
            State = InstallState.Failure;
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            UpdateVisibility();
        }
    }

    private bool CanInstall()
        => State == InstallState.Configuration && ModlistPicker.IsValid;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    private bool CanCancel() => State == InstallState.Installing;

    [RelayCommand]
    private void Done()
    {
        Summary = null;
        ErrorMessage = null;
        State = InstallState.Configuration;
    }

    partial void OnStateChanged(InstallState value)
    {
        UpdateVisibility();
        InstallCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private void UpdateVisibility()
    {
        IsConfiguring = State == InstallState.Configuration;
        IsInstalling = State == InstallState.Installing;
        IsSuccess = State == InstallState.Success;
        IsFailure = State == InstallState.Failure;
    }
}
