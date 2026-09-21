using FluentAssertions;
using Firelink.Core.Archives;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest;
using Firelink.Core.Models.Manifest.Sources;
using Firelink.Install.Downloaders;
using Firelink.Install.Steps;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firelink.Install.Tests;

public class SyncArchivesStepTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _downloadsPath;
    private readonly FileHashCache _hashCache = new();

    public SyncArchivesStepTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(), "firelink-install-sa-" + Guid.NewGuid());
        _downloadsPath = Path.Combine(_tempDir, "downloads");
        Directory.CreateDirectory(_downloadsPath);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private SyncArchivesStep MakeStep(params FakeArchiveDownloader[] downloaders)
    {
        var registry = new DownloaderRegistry(downloaders.Cast<Core.Abstractions.IArchiveDownloader>());
        return new SyncArchivesStep(
            registry, _hashCache, NullLogger<SyncArchivesStep>.Instance);
    }

    private static ModlistManifest MakeManifest(params ArchiveEntry[] archives)
    {
        return new ModlistManifest
        {
            SchemaVersion = "1.0.0",
            ManifestVersion = "1.0.0",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "firelink-pack/0.1.0",
            Meta = new ManifestMeta
            {
                Name = "Test",
                Version = "1.0.0",
                Author = "t",
                Game = "skyrimspecialedition",
                GameVersion = "1.6.1170",
            },
            Execution = new ExecutionPolicy(),
            Mo2 = new Mo2Section
            {
                Version = "2.5.2",
                Profile = "Default",
                Archive = new ArchiveEntry
                {
                    Id = "mo2",
                    Name = "MO2.7z",
                    Size = 0,
                    Hash = new XxHash64Value(0),
                    Sources = Array.Empty<ArchiveSourceRef>(),
                },
                Extensions = Array.Empty<ExtensionEntry>(),
            },
            StockGame = new StockGameSection { Extras = Array.Empty<ExtensionEntry>() },
            Archives = archives,
            Mods = Array.Empty<ModEntry>(),
            Plugins = Array.Empty<PluginEntry>(),
            Loadorder = Array.Empty<string>(),
        };
    }

    private static ArchiveEntry MakeMirrorArchive(string name, byte[] content, string url = "https://example.com/file.7z")
    {
        var hash = FakeArchiveDownloader.HashOf(content);
        return new ArchiveEntry
        {
            Id = $"local_{name}",
            Name = name,
            Size = content.Length,
            Hash = hash,
            Sources = new ArchiveSourceRef[]
            {
                new MirrorSourceRef { Url = url, Hash = hash },
            },
        };
    }

    private static SyncArchivesStep.Input MakeInput(ModlistManifest manifest, string downloadsPath, int maxParallel = 4)
        => new()
        {
            Manifest = manifest,
            DownloadsPath = downloadsPath,
            ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxParallel },
        };

    [Fact]
    public async Task Execute_ArchiveAlreadyInDownloads_IsAlreadyPresent()
    {
        var content = FakeArchiveDownloader.MakeBytes("fake archive");
        var archive = MakeMirrorArchive("Test.7z", content);

        File.WriteAllBytes(Path.Combine(_downloadsPath, "Test.7z"), content);

        var mirrorDownloader = new FakeArchiveDownloader("mirror");
        var step = MakeStep(mirrorDownloader);

        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        output.AlreadyPresent.Should().ContainSingle().Which.Should().Be("Test.7z");
        output.Downloaded.Should().BeEmpty();
        mirrorDownloader.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task Execute_ArchiveWithDifferentNameButSameHash_IsAlreadyPresent()
    {
        var content = FakeArchiveDownloader.MakeBytes("fake archive");
        var archive = MakeMirrorArchive("Expected.7z", content);

        File.WriteAllBytes(Path.Combine(_downloadsPath, "Different.7z"), content);

        var step = MakeStep(new FakeArchiveDownloader("mirror"));
        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        output.AlreadyPresent.Should().ContainSingle().Which.Should().Be("Expected.7z");
    }

    [Fact]
    public async Task Execute_MissingMirrorArchive_Downloads()
    {
        var content = FakeArchiveDownloader.MakeBytes("mirror content");
        var archive = MakeMirrorArchive("M.7z", content);

        var mirrorDownloader = new FakeArchiveDownloader("mirror");
        mirrorDownloader.SetContent(
            FakeArchiveDownloader.IdentifierOf(archive.Sources[0]), content);

        var step = MakeStep(mirrorDownloader);
        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        output.Downloaded.Should().ContainSingle().Which.Should().Be("M.7z");
    }

    [Fact]
    public async Task Execute_MultipleArchives_ProcessedInParallel()
    {
        var archives = new List<ArchiveEntry>();
        var mirrorDownloader = new FakeArchiveDownloader("mirror");

        for (int i = 0; i < 5; i++)
        {
            var content = FakeArchiveDownloader.MakeBytes($"content-{i}");
            var archive = MakeMirrorArchive($"File{i}.7z", content,
                url: $"https://example.com/file{i}.7z");
            archives.Add(archive);
            mirrorDownloader.SetContent(
                FakeArchiveDownloader.IdentifierOf(archive.Sources[0]), content);
        }

        var step = MakeStep(mirrorDownloader);
        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(archives.ToArray()), _downloadsPath),
            CancellationToken.None);

        output.Downloaded.Should().HaveCount(5);
        output.AlreadyPresent.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_NexusSourceWithoutDownloader_SkipsWithWarning()
    {
        var content = FakeArchiveDownloader.MakeBytes("nexus content");
        var hash = FakeArchiveDownloader.HashOf(content);
        var archive = new ArchiveEntry
        {
            Id = "nexus_skyrimspecialedition_1_1",
            Name = "Nexus.7z",
            Size = content.Length,
            Hash = hash,
            Sources = new ArchiveSourceRef[]
            {
                new NexusSourceRef { Game = "skyrimspecialedition", ModId = 1, FileId = 1 },
            },
        };

        var step = MakeStep(new FakeArchiveDownloader("mirror"));

        var act = async () => await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Nexus.7z*");
    }

    [Fact]
    public async Task Execute_NexusPlusMirrorSource_FallsBackToMirror()
    {
        var content = FakeArchiveDownloader.MakeBytes("dual content");
        var hash = FakeArchiveDownloader.HashOf(content);

        var archive = new ArchiveEntry
        {
            Id = "dual",
            Name = "Dual.7z",
            Size = content.Length,
            Hash = hash,
            Sources = new ArchiveSourceRef[]
            {
                new NexusSourceRef { Game = "skyrimspecialedition", ModId = 1, FileId = 1 },
                new MirrorSourceRef { Url = "https://example.com/file.7z", Hash = hash },
            },
        };

        var mirrorDownloader = new FakeArchiveDownloader("mirror");
        mirrorDownloader.SetContent("mirror:https://example.com/file.7z", content);

        var step = MakeStep(mirrorDownloader);
        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        output.Downloaded.Should().ContainSingle().Which.Should().Be("Dual.7z");
    }

    [Fact]
    public async Task Execute_NoDownloaderForOnlySource_Throws()
    {
        var content = FakeArchiveDownloader.MakeBytes("x");
        var hash = FakeArchiveDownloader.HashOf(content);
        var archive = new ArchiveEntry
        {
            Id = "nexus_1_1",
            Name = "N.7z",
            Size = content.Length,
            Hash = hash,
            Sources = new ArchiveSourceRef[]
            {
                new NexusSourceRef { Game = "g", ModId = 1, FileId = 1 },
            },
        };

        var step = MakeStep();
        var act = async () => await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*N.7z*");
    }

    [Fact]
    public async Task Execute_HashMismatchAfterDownload_Throws()
    {
        var expectedContent = FakeArchiveDownloader.MakeBytes("expected");
        var wrongContent = FakeArchiveDownloader.MakeBytes("wrong!");

        var archive = MakeMirrorArchive("Hash.7z", expectedContent);

        var mirrorDownloader = new FakeArchiveDownloader("mirror");
        mirrorDownloader.SetContent(
            FakeArchiveDownloader.IdentifierOf(archive.Sources[0]),
            wrongContent);

        var step = MakeStep(mirrorDownloader);

        var act = async () => await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Hash.7z*");
    }

    [Fact]
    public async Task Execute_ArchiveWithNoSources_Throws()
    {
        var archive = new ArchiveEntry
        {
            Id = "no-sources",
            Name = "NoSources.7z",
            Size = 100,
            Hash = new XxHash64Value(0xabc),
            Sources = Array.Empty<ArchiveSourceRef>(),
        };

        var step = MakeStep(new FakeArchiveDownloader("mirror"));
        var act = async () => await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*NoSources.7z*");
    }

    [Fact]
    public async Task Execute_RetryAfterTransientFailure_Succeeds()
    {
        var content = FakeArchiveDownloader.MakeBytes("retry content");
        var archive = MakeMirrorArchive("Retry.7z", content);

        var mirrorDownloader = new FakeArchiveDownloader("mirror");
        mirrorDownloader.SetContent(
            FakeArchiveDownloader.IdentifierOf(archive.Sources[0]), content);
        mirrorDownloader.FailOnCall(1);

        var step = MakeStep(mirrorDownloader);
        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        output.Downloaded.Should().ContainSingle().Which.Should().Be("Retry.7z");
        mirrorDownloader.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task Execute_FailedDownload_LeavesNoPartFile()
    {
        var content = FakeArchiveDownloader.MakeBytes("x");
        var wrongContent = FakeArchiveDownloader.MakeBytes("y");
        var archive = MakeMirrorArchive("Failed.7z", content);

        var mirrorDownloader = new FakeArchiveDownloader("mirror");
        mirrorDownloader.SetContent(
            FakeArchiveDownloader.IdentifierOf(archive.Sources[0]), wrongContent);

        var step = MakeStep(mirrorDownloader);
        var act = async () => await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        Directory.GetFiles(_downloadsPath, "*.part", SearchOption.TopDirectoryOnly)
            .Should().BeEmpty();

        File.Exists(Path.Combine(_downloadsPath, "Failed.7z")).Should().BeFalse();
    }

    [Fact]
    public async Task Execute_IgnoresPartAndMetaFiles()
    {
        var content = FakeArchiveDownloader.MakeBytes("x");
        var archive = MakeMirrorArchive("Real.7z", content);

        File.WriteAllText(Path.Combine(_downloadsPath, "garbage.part"), "junk");
        File.WriteAllText(Path.Combine(_downloadsPath, "some.meta"), "junk");
        File.WriteAllText(Path.Combine(_downloadsPath, "readme.txt"), "junk");

        var mirrorDownloader = new FakeArchiveDownloader("mirror");
        mirrorDownloader.SetContent(
            FakeArchiveDownloader.IdentifierOf(archive.Sources[0]), content);

        var step = MakeStep(mirrorDownloader);
        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(archive), _downloadsPath),
            CancellationToken.None);

        output.Downloaded.Should().ContainSingle().Which.Should().Be("Real.7z");
    }

    [Fact]
    public async Task Execute_EmptyManifest_ReturnsEmptyOutput()
    {
        var step = MakeStep();
        var output = await step.ExecuteAsync(
            MakeInput(MakeManifest(), _downloadsPath),
            CancellationToken.None);

        output.AlreadyPresent.Should().BeEmpty();
        output.Downloaded.Should().BeEmpty();
        output.Skipped.Should().BeEmpty();
    }
}
