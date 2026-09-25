using Firelink.Gui.Shared.Services;

namespace Firelink.Gui.Shared.Tests.Fakes;

public sealed class FakeProcessLauncher : IProcessLauncher
{
    public List<string> OpenedPaths { get; } = new();
    public Exception? ExceptionToThrow { get; set; }

    public void OpenFile(string path)
    {
        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        OpenedPaths.Add(path);
    }
}
