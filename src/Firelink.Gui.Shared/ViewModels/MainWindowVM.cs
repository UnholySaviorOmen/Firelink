using CommunityToolkit.Mvvm.ComponentModel;
using Firelink.Gui.Shared.Navigation;

namespace Firelink.Gui.Shared.ViewModels;

public sealed partial class MainWindowVM : ViewModel
{
    private readonly IScreenFactory _screens;

    [ObservableProperty]
    private object? _activePane;

    public NavigationVM Navigation { get; }
    public HomeVM Home { get; }

    public MainWindowVM(IScreenFactory screens, SettingsVM settings)
    {
        _screens = screens;

        Home = (HomeVM)_screens.Create(ScreenType.Home);
        Navigation = new NavigationVM(NavigateTo, settings);

        _activePane = Home;
        Navigation.SelectScreen(ScreenType.Home);
        Home.Refresh();
    }

    public void NavigateTo(ScreenType screen)
    {
        var pane = _screens.Create(screen);

        ActivePane = pane;
        Navigation.SelectScreen(screen);

        if (pane is HomeVM home)
        {
            home.InstallRequested -= OnInstallRequested;
            home.InstallRequested += OnInstallRequested;
            home.Refresh();
        }
    }

    private void OnInstallRequested(string manifestPath, string targetPath)
    {
        NavigateTo(ScreenType.Install);

        if (ActivePane is IInstallTarget target)
            target.PrepareForInstall(manifestPath, targetPath);
    }
}
