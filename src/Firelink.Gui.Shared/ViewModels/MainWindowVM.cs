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

    public MainWindowVM(IScreenFactory screens)
    {
        _screens = screens;

        Home = (HomeVM)_screens.Create(ScreenType.Home);
        Navigation = new NavigationVM(NavigateTo);

        _activePane = Home;
        Navigation.SelectScreen(ScreenType.Home);
    }

    public void NavigateTo(ScreenType screen)
    {
        var pane = _screens.Create(screen);

        if (pane is INavigationAware nav)
            nav.SetNavigateHome(() => NavigateTo(ScreenType.Home));

        ActivePane = pane;

        Navigation.SelectScreen(screen);
    }
}
