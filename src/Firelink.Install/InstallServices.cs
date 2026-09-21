using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Install.Downloaders;
using Firelink.Install.Steps;
using Firelink.Install.Verify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firelink.Install;

/// <summary>
/// DI-extension: регистрирует все сервисы installer-а.
///
/// Вызывается из Firelink.Cli/Program.cs (и в будущем — из GUI).
/// Использует TryAddSingleton, чтобы не плодить дубли: FileHashCache
/// и IArchiveExtractor регистрируются и в AddFirelinkPack тоже.
///
/// AddHttpClient<MirrorDownloader> требует пакет
/// Microsoft.Extensions.Http — он явно добавлен в
/// Firelink.Install.csproj.
///
/// НЕ регистрирует:
///   - IAnsiConsole (это CLI/GUI);
///   - ParallelOptions (это клиент решает);
///   - ILogger<T> (это AddLogging в клиенте).
/// </summary>
public static class InstallServices
{
    public static IServiceCollection AddFirelinkInstall(this IServiceCollection services)
    {
        // --- Core-сервисы, общие с packer ---
        services.TryAddSingleton<FileHashCache>();
        services.TryAddSingleton<IArchiveExtractor, SevenZipExtractor>();

        // --- Downloaders ---
        services.AddHttpClient<MirrorDownloader>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        services.TryAddSingleton<IArchiveDownloader>(sp =>
            sp.GetRequiredService<MirrorDownloader>());

        services.TryAddSingleton<DownloaderRegistry>();

        // --- Install steps ---
        services.TryAddSingleton<ReadManifestStep>();
        services.TryAddSingleton<ResolveTargetStep>();
        services.TryAddSingleton<ValidateTargetStep>();
        services.TryAddSingleton<BootstrapInstanceStep>();
        services.TryAddSingleton<BootstrapMo2Step>();
        services.TryAddSingleton<SyncArchivesStep>();
        services.TryAddSingleton<ExecuteExtensionsStep>();
        services.TryAddSingleton<ExecuteExtrasStep>();
        services.TryAddSingleton<SyncModsStep>();
        services.TryAddSingleton<GenerateMetaIniStep>();
        services.TryAddSingleton<RegenerateProfileStep>();

        // --- Verify ---
        services.TryAddSingleton<VerifyPipeline>();

        // --- Pipeline ---
        services.TryAddSingleton<InstallPipeline>();

        return services;
    }
}
