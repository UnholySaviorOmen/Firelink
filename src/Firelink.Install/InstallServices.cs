using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Install.Downloaders;
using Firelink.Install.Steps;
using Firelink.Install.Verify;
using Firelink.Platform.Nexus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firelink.Install;

/// <summary>
/// DI-extension: регистрирует все сервисы installer-а.
///
/// Скачивание:
///   - MirrorDownloader и NexusDownloader регистрируются через
///     TryAddEnumerable(ServiceDescriptor.Singleton&lt;IArchiveDownloader, TConcrete&gt;()).
///     Это даёт: (а) обе реализации попадают в DownloaderRegistry;
///     (б) повторный вызов AddFirelinkInstall не плодит дубликаты.
///   - HttpClient-ы берутся downloader-ами из IHttpClientFactory по имени:
///       "mirror"    — 10 минут.
///       "nexus"     — 10 минут.
///       "nexus-api" — 2 минуты.
///
/// ВАЖНО: downloader-ы принимают IHttpClientFactory, а не HttpClient.
/// Иначе DI создаёт их через активацию конструктора и подставит
/// безымянный HttpClient с дефолтным таймаутом (100 секунд). Для
/// гигабайтных архивов с Nexus это критично — 100 секунд не хватит.
///
/// НЕ регистрирует:
///   - IAnsiConsole (это CLI/GUI);
///   - ParallelOptions (это клиент решает);
///   - ILogger&lt;T&gt; (это AddLogging в клиенте).
/// </summary>
public static class InstallServices
{
    public static IServiceCollection AddFirelinkInstall(this IServiceCollection services)
    {
        // --- Core-сервисы, общие с packer ---
        services.TryAddSingleton<FileHashCache>();
        services.TryAddSingleton<IArchiveExtractor, SevenZipExtractor>();

        // --- Nexus ---
        services.TryAddSingleton<INexusApiKeyProvider, NexusApiKeyProvider>();

        // --- HttpClient-ы (именованные) ---
        services.AddHttpClient(MirrorDownloader.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        services.AddHttpClient(NexusDownloader.HttpClientName, client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        services.AddHttpClient("nexus-api", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        // --- NexusClient (API) ---
        services.TryAddSingleton<NexusClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new NexusClient(
                factory.CreateClient("nexus-api"),
                sp.GetRequiredService<INexusApiKeyProvider>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<NexusClient>>());
        });

        // --- Downloaders в коллекции IArchiveDownloader ---
        // TryAddEnumerable с явным ImplementationType — каждая пара
        // (ServiceType, ImplementationType) регистрируется один раз,
        // даже если AddFirelinkInstall вызывается дважды.
        //
        // DI создаст downloader-ы через активацию конструктора.
        // Конструкторы принимают IHttpClientFactory — не HttpClient —
        // чтобы получить именованный клиент с правильным таймаутом.
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IArchiveDownloader, MirrorDownloader>());

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IArchiveDownloader, NexusDownloader>());

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
