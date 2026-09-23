using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Firelink.Gui.Shared.Services;

namespace Firelink.Gui.Services;

public sealed class AvaloniaFilePickerService : IFilePickerService
{
    public async Task<string?> PickFileAsync(string title, string? filterHint = null)
    {
        var window = GetMainWindow();
        if (window is null) return null;

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        };

        if (!string.IsNullOrWhiteSpace(filterHint))
        {
            var pattern = filterHint.StartsWith('.') ? "*" + filterHint : filterHint;
            options.FileTypeFilter = new[]
            {
                new FilePickerFileType(filterHint) { Patterns = new[] { pattern } },
            };
        }

        var files = await window.StorageProvider.OpenFilePickerAsync(options);
        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public async Task<string?> PickFolderAsync(string title)
    {
        var window = GetMainWindow();
        if (window is null) return null;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }
}
