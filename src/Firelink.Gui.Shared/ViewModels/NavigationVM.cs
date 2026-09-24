using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Firelink.Gui.Shared.Navigation;

namespace Firelink.Gui.Shared.ViewModels;

public sealed partial class NavigationVM : ViewModel
{
    private readonly Action<ScreenType> _navigate;
    private bool _suppressCallback;

    public ObservableCollection<NavigationItem> Items { get; }

    [ObservableProperty]
    private NavigationItem? _selectedItem;

    public NavigationVM(Action<ScreenType> navigate)
    {
        _navigate = navigate;
        Items = new ObservableCollection<NavigationItem>
        {
            new("Home", ScreenType.Home),
            new("Install", ScreenType.Install),
            new("Pack", ScreenType.Pack),
            new("Verify", ScreenType.Verify),
            new("Logs", ScreenType.Logs),
            new("Settings", ScreenType.Settings),
        };
    }

    public void SelectScreen(ScreenType screen)
    {
        var item = Items.FirstOrDefault(i => i.Screen == screen);
        if (item is null) return;

        _suppressCallback = true;
        SelectedItem = item;
        _suppressCallback = false;
    }

    partial void OnSelectedItemChanged(NavigationItem? value)
    {
        if (_suppressCallback) return;
        if (value is null) return;

        _navigate(value.Screen);
    }
}
