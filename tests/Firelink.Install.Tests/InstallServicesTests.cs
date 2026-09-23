using FluentAssertions;
using Firelink.Core.Abstractions;
using Firelink.Install.Downloaders;
using Firelink.Platform.Nexus;
using Microsoft.Extensions.DependencyInjection;

namespace Firelink.Install.Tests;

/// <summary>
/// Проверяет DI-регистрацию downloader-ов: оба (mirror и nexus) должны
/// попадать в DownloaderRegistry.
///
/// Историческая причина существования этих тестов: MirrorDownloader был
/// зарегистрирован через TryAddSingleton&lt;IArchiveDownloader&gt;, что при
/// добавлении второго downloader-а молча теряло бы один из них.
/// TryAddEnumerable — фикс; тест держит регрессию.
/// </summary>
public class InstallServicesTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFirelinkInstall();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void BothDownloadersAreResolvableThroughTheInterface()
    {
        using var sp = BuildProvider();

        var all = sp.GetServices<IArchiveDownloader>().ToList();

        all.Should().HaveCount(2);
        all.Should().ContainSingle(d => d.SourceType == "mirror");
        all.Should().ContainSingle(d => d.SourceType == "nexus");
    }

    [Fact]
    public void BothDownloadersAreRegisteredAsIArchiveDownloader()
    {
        using var sp = BuildProvider();

        var all = sp.GetServices<IArchiveDownloader>().ToList();

        all.Should().ContainSingle(d => d is MirrorDownloader);
        all.Should().ContainSingle(d => d is NexusDownloader);
    }

    [Fact]
    public void DownloaderRegistryContainsBothSourceTypes()
    {
        using var sp = BuildProvider();

        var registry = sp.GetRequiredService<DownloaderRegistry>();

        registry.RegisteredTypes.Should().Contain("mirror");
        registry.RegisteredTypes.Should().Contain("nexus");
    }

    [Fact]
    public void CallingAddFirelinkInstallTwiceDoesNotDuplicate()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFirelinkInstall();
        services.AddFirelinkInstall();

        using var sp = services.BuildServiceProvider();

        var all = sp.GetServices<IArchiveDownloader>().ToList();

        all.Should().ContainSingle(d => d is MirrorDownloader);
        all.Should().ContainSingle(d => d is NexusDownloader);
    }

    [Fact]
    public void NexusApiKeyProviderIsRegistered()
    {
        using var sp = BuildProvider();

        var provider = sp.GetRequiredService<INexusApiKeyProvider>();
        provider.Should().BeOfType<NexusApiKeyProvider>();
    }

    [Fact]
    public void NexusClientIsRegistered()
    {
        using var sp = BuildProvider();

        sp.GetRequiredService<NexusClient>().Should().NotBeNull();
    }
}
