using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Pack.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firelink.Pack;

/// <summary>
/// DI-extension: регистрирует все сервисы packer-а.
///
/// Вызывается из Firelink.Cli/Program.cs (и в будущем — из GUI).
/// Использует TryAddSingleton, чтобы не плодить дубли: FileHashCache
/// и IArchiveExtractor регистрируются и в AddFirelinkInstall тоже.
///
/// НЕ регистрирует:
///   - IAnsiConsole (это CLI/GUI);
///   - ParallelOptions (это клиент решает);
///   - ILogger<T> (это AddLogging в клиенте);
///   - VerifyPipeline (это installer).
/// </summary>
public static class PackServices
{
    public static IServiceCollection AddFirelinkPack(this IServiceCollection services)
    {
        // --- Core-сервисы, общие с installer ---
        services.TryAddSingleton<FileHashCache>();
        services.TryAddSingleton<IArchiveExtractor, SevenZipExtractor>();

        // --- Pack steps ---
        services.TryAddSingleton<ReadConfigStep>();
        services.TryAddSingleton<ReadInstanceStep>();
        services.TryAddSingleton<IndexArchivesStep>();
        services.TryAddSingleton<ScanModsStep>();
        services.TryAddSingleton<ScanExtensionsStep>();
        services.TryAddSingleton<ScanExtrasStep>();
        services.TryAddSingleton<MatchStep>();
        services.TryAddSingleton<MatchExtensionsStep>();
        services.TryAddSingleton<MatchExtrasStep>();
        services.TryAddSingleton<BuildManifestStep>();
        services.TryAddSingleton<ValidateManifestStep>();
        services.TryAddSingleton<WriteManifestStep>();

        services.TryAddSingleton<PackPipeline>();

        return services;
    }
}
