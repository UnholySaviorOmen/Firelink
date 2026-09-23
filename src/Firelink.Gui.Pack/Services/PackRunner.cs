using Firelink.Core.Progress;
using Firelink.Pack;

namespace Firelink.Gui.Pack.Services;

public sealed class PackRunner : IPackRunner
{
    private readonly PackPipeline _pipeline;

    public PackRunner(PackPipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public async Task<PackSummary> RunAsync(
        string configPath,
        IProgress<StepProgress> progress,
        CancellationToken ct)
    {
        var input = PackInputFactory.Create(configPath);

        var result = await _pipeline.ExecuteAsync(input, ct, progress);

        return PackSummaryBuilder.Build(result);
    }
}
