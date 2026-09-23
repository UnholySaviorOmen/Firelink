using CommunityToolkit.Mvvm.ComponentModel;

namespace Firelink.Gui.Shared.ViewModels;

public abstract class ViewModel : ObservableObject
{
    public virtual void Dispose() { }
}
