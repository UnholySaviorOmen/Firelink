using Firelink.Install.Verify;

namespace Firelink.Gui.Verify.Services;

public sealed class VerifyRunner : IVerifyRunner
{
    private readonly VerifyPipeline _pipeline;

    public VerifyRunner(VerifyPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public Task<VerifyReport> RunAsync(string targetPath, CancellationToken ct)
    {
        // VerifyPipeline.Execute — синхронный. Task.Run уводит работу
        // с UI-потока; ct пробрасывается внутрь Execute, где есть
        // ThrowIfCancellationRequested в начале и в циклах.
        return Task.Run(() => _pipeline.Execute(targetPath, ct), ct);
    }
}
