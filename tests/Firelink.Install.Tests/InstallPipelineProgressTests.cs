using System.IO.Compression;
using System.Text;
using Firelink.Core.Abstractions;
using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest;
using Firelink.Core.Progress;
using Firelink.Install;
using Firelink.Install.Downloaders;
using Firelink.Install.Steps;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firelink.Install.Tests;

/// <summary>
/// Проверяет, что InstallPipeline корректно репортит IProgress&lt;StepProgress&gt;.
///
/// Строит минимальный манифест + готовую структуру target-инстанса
/// с уже разложенными архивами и проверяет:
///   - 11 репортов (по числу шагов);
///   - StepIndex идёт 1..11;
///   - TotalSteps == 11;
///   - StepName непустой;
///   - первый — "ReadManifest", последний — "RegenerateProfile";
///   - если progress == null, pipeline всё равно работает.
/// </summary>
public class InstallPipelineProgressTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _sourceDir;
    private readonly string _targetDir;
    private readonly FileHashCache _hashCache = new();

    public InstallPipelineProgressTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(), "firelink-install-progress-" + Guid.NewGuid());
        _sourceDir = Path.Combine(_tempDir, "source");
        _targetDir = Path.Combine(_tempDir, "target");

        Directory.CreateDirectory(_sourceDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private (string path, ModlistManifest manifest, string mo2Hash)
        PrepareManifestAndDownloads()
    {
        // Готовим target/MO2/downloads заранее, чтобы installer не качал.
        var downloadsDir = Path.Combine(_targetDir, "MO2", "downloads");
        Directory.CreateDirectory(downloadsDir);

        // --- MO2-архив ---
        var mo2ArchivePath = Path.Combine(
            downloadsDir, "Mod.Organizer-2.5.2.7z");
        var mo2ExeContent = Encoding.UTF8.GetBytes("fake MO2 exe content");
        using (var fs = File.Create(mo2ArchivePath))
        using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("ModOrganizer.exe");
            using var es = entry.Open();
            es.Write(mo2ExeContent, 0, mo2ExeContent.Length);
        }
        var mo2Hash = _hashCache.GetOrCompute(mo2ArchivePath);
        var mo2Size = new FileInfo(mo2ArchivePath).Length;

        // --- Mod-архив ---
        var modArchivePath = Path.Combine(downloadsDir, "TestMod.7z");
        var modFileContent = Encoding.UTF8.GetBytes("hello from mod");
        using (var fs = File.Create(modArchivePath))
        using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
        {
            var entry = zip.CreateEntry("file.txt");
            using var es = entry.Open();
            es.Write(modFileContent, 0, modFileContent.Length);
        }
        var modArchiveHash = _hashCache.GetOrCompute(modArchivePath);
        var modArchiveSize = new FileInfo(modArchivePath).Length;
        var modFileHash = XxHash64Value.FromStream(new MemoryStream(modFileContent));

        // --- Манифест ---
        var manifest = new Core.Models.Manifest.ModlistManifest
        {
            SchemaVersion = "1.0.0",
            ManifestVersion = "1.0.0",
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "firelink-pack/0.1.0",
            Meta = new Core.Models.Manifest.ManifestMeta
            {
                Name = "Progress Test Pack",
                Version = "1.0.0",
                Author = "tester",
                Game = "skyrimspecialedition",
                GameVersion = "1.6.1170",
            },
            Execution = new Core.Models.Manifest.ExecutionPolicy(),
            Mo2 = new Core.Models.Manifest.Mo2Section
            {
                Version = "2.5.2",
                Profile = "Default",
                Archive = new Core.Models.Manifest.ArchiveEntry
                {
                    Id = "local_mod-organizer-2-5-2",
                    Name = "Mod.Organizer-2.5.2.7z",
                    Size = mo2Size,
                    Hash = mo2Hash,
                    Sources = new Core.Models.Manifest.Sources.ArchiveSourceRef[]
                    {
                        new Core.Models.Manifest.Sources.MirrorSourceRef
                        {
                            Url = "https://example.com/Mod.Organizer-2.5.2.7z",
                            Hash = mo2Hash,
                        },
                    },
                },
                Extensions = Array.Empty<Core.Models.Manifest.ExtensionEntry>(),
            },
            StockGame = new Core.Models.Manifest.StockGameSection
            {
                Extras = Array.Empty<Core.Models.Manifest.ExtensionEntry>(),
            },
            Archives = new[]
            {
                new Core.Models.Manifest.ArchiveEntry
                {
                    Id = "local_testmod",
                    Name = "TestMod.7z",
                    Size = modArchiveSize,
                    Hash = modArchiveHash,
                    Sources = new Core.Models.Manifest.Sources.ArchiveSourceRef[]
                    {
                        new Core.Models.Manifest.Sources.MirrorSourceRef
                        {
                            Url = "https://example.com/TestMod.7z",
                            Hash = modArchiveHash,
                        },
                    },
                },
            },
            Mods = new[]
            {
                new Core.Models.Manifest.ModEntry
                {
                    Name = "TestMod",
                    Enabled = true,
                    Order = 0,
                    Meta = null,
                    Directives = new Core.Models.Manifest.Directives.Directive[]
                    {
                        new Core.Models.Manifest.Directives.FromArchiveDirective
                        {
                            Archive = "local_testmod",
                            Source = "file.txt",
                            Destination = "file.txt",
                            Hash = modFileHash,
                            Size = modFileContent.Length,
                        },
                    },
                },
            },
            Plugins = Array.Empty<Core.Models.Manifest.PluginEntry>(),
            Loadorder = Array.Empty<string>(),
        };

        var manifestPath = Path.Combine(_sourceDir, "modlist.json");
        File.WriteAllText(manifestPath, Core.Models.Manifest.ManifestJson.Serialize(manifest));

        return (manifestPath, manifest, mo2Hash.Value.ToString("x16"));
    }

    private static InstallPipeline BuildPipeline()
    {
        var hashCache = new FileHashCache();
        var extractor = new SevenZipExtractor(
            NullLogger<SevenZipExtractor>.Instance);
        var registry = new DownloaderRegistry(
            Array.Empty<IArchiveDownloader>());

        return new InstallPipeline(
            new ReadManifestStep(NullLogger<ReadManifestStep>.Instance),
            new ResolveTargetStep(NullLogger<ResolveTargetStep>.Instance),
            new ValidateTargetStep(NullLogger<ValidateTargetStep>.Instance),
            new BootstrapInstanceStep(NullLogger<BootstrapInstanceStep>.Instance),
            new BootstrapMo2Step(
                registry, hashCache, extractor,
                NullLogger<BootstrapMo2Step>.Instance),
            new SyncArchivesStep(
                registry, hashCache,
                NullLogger<SyncArchivesStep>.Instance),
            new ExecuteExtensionsStep(
                extractor,
                NullLogger<ExecuteExtensionsStep>.Instance),
            new ExecuteExtrasStep(
                extractor,
                NullLogger<ExecuteExtrasStep>.Instance),
            new SyncModsStep(
                extractor, hashCache,
                NullLogger<SyncModsStep>.Instance),
            new GenerateMetaIniStep(
                NullLogger<GenerateMetaIniStep>.Instance),
            new RegenerateProfileStep(
                NullLogger<RegenerateProfileStep>.Instance),
            NullLogger<InstallPipeline>.Instance);
    }

    private sealed class ListProgress : IProgress<StepProgress>
    {
        public List<StepProgress> Reports { get; } = new();
        public void Report(StepProgress value) => Reports.Add(value);
    }

    [Fact]
    public async Task Execute_WithProgress_ReportsAllSteps()
    {
        var (manifestPath, _, _) = PrepareManifestAndDownloads();
        var pipeline = BuildPipeline();
        var progress = new ListProgress();

        await pipeline.ExecuteAsync(
            new InstallPipeline.Input
            {
                ManifestPath = manifestPath,
                Target = _targetDir,
                ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1 },
            },
            CancellationToken.None,
            progress);

        // 11 шагов.
        progress.Reports.Should().HaveCount(11);

        progress.Reports.Select(r => r.StepIndex)
            .Should().Equal(Enumerable.Range(1, 11));

        progress.Reports.Select(r => r.TotalSteps)
            .Should().AllBeEquivalentTo(11);

        progress.Reports.Select(r => r.StepName)
            .Should().NotContainNulls()
            .And.OnlyContain(s => !string.IsNullOrWhiteSpace(s));

        progress.Reports[0].StepName.Should().Be("ReadManifest");
        progress.Reports[^1].StepName.Should().Be("RegenerateProfile");
    }

    [Fact]
    public async Task Execute_WithoutProgress_StillWorks()
    {
        var (manifestPath, _, _) = PrepareManifestAndDownloads();
        var pipeline = BuildPipeline();

        var output = await pipeline.ExecuteAsync(
            new InstallPipeline.Input
            {
                ManifestPath = manifestPath,
                Target = _targetDir,
                ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1 },
            },
            CancellationToken.None);

        output.Manifest.Meta.Name.Should().Be("Progress Test Pack");
        Directory.Exists(output.InstancePath).Should().BeTrue();
    }
}
