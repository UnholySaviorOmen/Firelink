using Firelink.Core.Abstractions;
using Firelink.Core.Models.Pack;
using Firelink.Core.Validation;
using Microsoft.Extensions.Logging;

namespace Firelink.Pack.Steps;

/// <summary>
/// Читает firelink-pack.json, валидирует через PackConfigValidator.
/// Возвращает PackConfig или бросает InvalidOperationException с полным списком ошибок.
/// </summary>
public sealed class ReadConfigStep : IStep<string, PackConfig>
{
    private readonly ILogger<ReadConfigStep> _logger;

    public ReadConfigStep(ILogger<ReadConfigStep> logger)
    {
        _logger = logger;
    }

    public async Task<PackConfig> ExecuteAsync(string configPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(configPath))
            throw new ArgumentException("Config path must be non-empty.", nameof(configPath));

        if (!File.Exists(configPath))
            throw new FileNotFoundException(
                $"firelink-pack.json not found: {configPath}", configPath);

        _logger.LogInformation("Reading config: {Path}", configPath);

        PackConfig config;
        try
        {
            config = await PackConfigJson.LoadAsync(configPath, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"Failed to parse firelink-pack.json: {ex.Message}", ex);
        }

        var validation = PackConfigValidator.Validate(config);
        if (!validation.IsValid)
        {
            var msg = "firelink-pack.json is invalid:" + Environment.NewLine +
                      string.Join(Environment.NewLine, validation.Errors.Select(e => "  - " + e));
            throw new InvalidOperationException(msg);
        }

        _logger.LogInformation(
            "Config OK: {Name} v{Version} ({Game})",
            config.Meta.Name, config.Meta.Version, config.Meta.Game);

        return config;
    }
}
