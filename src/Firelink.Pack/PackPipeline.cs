using Firelink.Core.Models.Pack;
using Firelink.Pack.Steps;
using Microsoft.Extensions.Logging;

namespace Firelink.Pack;

public sealed class PackPipeline
{
    private readonly ReadConfigStep _readConfig;
    private readonly ReadInstanceStep _readInstance;
    private readonly IndexArchivesStep _indexArchives;
    private readonly ScanModsStep _scanMods;
    private readonly MatchStep _match;
    private readonly ILogger<PackPipeline> _logger;

    public PackPipeline(
        ReadConfigStep readConfig,
        ReadInstanceStep readInstance,
        IndexArchivesStep indexArchives,
        ScanModsStep scanMods,
        MatchStep match,
        ILogger<PackPipeline> logger)
    {
        _readConfig = readConfig;
        _readInstance = readInstance;
        _indexArchives = indexArchives;
        _scanMods = scanMods;
        _match = match;
        _logger = logger;
    }

    public async Task<PackResult> ExecuteAsync(
        string configPath,
        ParallelOptions parallelOptions,
        CancellationToken ct)
    {
        _logger.LogInformation("=== Firelink pack started ===");
        _logger.LogInformation("Config: {Path}", configPath);

        var config = await _readConfig.ExecuteAsync(configPath, ct);

        var snapshot = await _readInstance.ExecuteAsync(
            new ReadInstanceStep.Input { ConfigPath = configPath, Config = config }, ct);

        var archiveIndex = await _indexArchives.ExecuteAsync(
            new IndexArchivesStep.Input
            {
                DownloadsPath = snapshot.DownloadsPath,
                Config = config,
                GameDomain = config.Meta.Game,
                ParallelOptions = parallelOptions,
            }, ct);

        var modScan = await _scanMods.ExecuteAsync(
            new ScanModsStep.Input
            {
                Snapshot = snapshot,
                ParallelOptions = parallelOptions,
            }, ct);

        var match = await _match.ExecuteAsync(
            new MatchStep.Input
            {
                ArchiveIndex = archiveIndex,
                ModScan = modScan,
                DownloadsPath = snapshot.DownloadsPath,
                ModsPath = snapshot.ModsPath,
                FirelinkOutputPath = snapshot.FirelinkOutputPath,
            }, ct);

        _logger.LogInformation("=== Firelink pack finished (partial) ===");

        return new PackResult
        {
            Config = config,
            Snapshot = snapshot,
            ArchiveIndex = archiveIndex,
            ModScan = modScan,
            Match = match,
        };
    }
}

public sealed class PackResult
{
    public required PackConfig Config { get; init; }
    public required InstanceSnapshot Snapshot { get; init; }
    public required ArchiveIndex ArchiveIndex { get; init; }
    public required ModScanResult ModScan { get; init; }
    public required MatchResult Match { get; init; }
}
