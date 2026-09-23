using Firelink.Gui.Shared.Services;

namespace Firelink.Gui.Install.Tests.Fakes;

public sealed class FakeFilePickerService : IFilePickerService
{
    public string? FileToReturn { get; set; }
    public string? FolderToReturn { get; set; }

    public Task<string?> PickFileAsync(string title, string? filterHint = null)
        => Task.FromResult(FileToReturn);

    public Task<string?> PickFolderAsync(string title)
        => Task.FromResult(FolderToReturn);
}
