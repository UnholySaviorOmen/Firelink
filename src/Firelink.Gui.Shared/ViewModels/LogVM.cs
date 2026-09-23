using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Firelink.Gui.Shared.Logging;

namespace Firelink.Gui.Shared.ViewModels;

public sealed partial class LogVM : ViewModel
{
    private readonly ObservableLogSink _sink;

    public LogVM(ObservableLogSink sink)
    {
        _sink = sink;
    }

    public ObservableCollection<LogEntry> Entries => _sink.Entries;

    [RelayCommand]
    public void Clear() => _sink.Clear();
}
