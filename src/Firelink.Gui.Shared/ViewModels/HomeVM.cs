using CommunityToolkit.Mvvm.ComponentModel;

namespace Firelink.Gui.Shared.ViewModels;

public sealed partial class HomeVM : ViewModel
{
    [ObservableProperty]
    private string _title = "Firelink";

    [ObservableProperty]
    private string _subtitle = "Reproducible modpack builds for Mod Organizer 2";

    public LogVM Log { get; }

    public HomeVM(LogVM log)
    {
        Log = log;
    }
}
