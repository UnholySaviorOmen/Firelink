using System.IO.Compression;
using FluentAssertions;
using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest;
using Firelink.Core.Models.Manifest.Directives;
using Firelink.Core.Models.Manifest.Sources;
using Firelink.Core.Models.Pack;
using Firelink.Pack.Steps;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firelink.Pack.Tests;

public class MatchStepTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _downloadsDir;
    private readonly string _modsDir;
    private readonly FileHashCache _hashCache = new();
    private readonly MatchStep _step;

    public MatchStepTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "firelink-match-" + Guid.NewGuid());
        _downloadsDir = Path.Combine(_tempDir, "downloads");
        _modsDir = Path.Combine(_tempDir, "mods");
        Directory.CreateDirectory(_downloadsDir);
        Directory.CreateDirectory(_modsDir);

        var extractor = new SevenZipExtractor(
            NullLogger<SevenZipExtractor>.Instance);
        _step = new MatchStep(extractor, _hashCache, NullLogger<MatchStep>.Instance);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private string CreateZip(string name, params (string entryPath, byte[] content)[] files)
    {
        var zipPath = Path.Combine(_downloadsDir, name);
        using var fs = File.Create(zipPath);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create);

        foreach (var (entryPath, content) in files)
        {
            var entry = zip.CreateEntry(entryPath);
            using var entryStream = entry.Open();
            entryStream.Write(content, 0, content.Length);
        }

        return zipPath;
    }

    private void WriteModFile(string modName, string relativePath, byte[] content)
    {
        var fullPath = Path.Combine(_modsDir, modName,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, content);
    }

    private static XxHash64Value HashOf(byte[] content)
    {
        using var ms = new MemoryStream(content);
        return XxHash64Value.FromStream(ms);
    }

    private ArchiveEntry MakeArchiveEntry(string id, string name)
        => new()
        {
            Id = id,
            Name = name,
            Size = new FileInfo(Path.Combine(_downloadsDir, name)).Length,
            Hash = _hashCache.GetOrCompute(Path.Combine(_downloadsDir, name)),
            Sources = new ArchiveSourceRef[]
            {
                new NexusSourceRef { ModId = 1, FileId = 1, Game = "skyrimspecialedition" },
            },
        };

    private MatchStep.Input MakeInput(
        ArchiveIndex archiveIndex,
        ModScanResult modScan) => new()
        {
            ArchiveIndex = archiveIndex,
            ModScan = modScan,
            DownloadsPath = _downloadsDir,
            ModsPath = _modsDir,
        };

    [Fact]
    public async Task Execute_FileInArchive_ProducesFromArchiveDirective()
    {
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var hash = HashOf(content);

        CreateZip("test.zip", ("interface/iconmenu.swf", content));
        WriteModFile("SkyUI", "interface/iconmenu.swf", content);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_1_1", "test.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>
            {
                ["SkyUI"] = new[]
                {
                    new ScannedFile
                    {
                        RelativePath = "interface/iconmenu.swf",
                        Hash         = hash,
                        Size         = content.Length,
                    },
                },
            },
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives["SkyUI"].Should().HaveCount(1);
        var directive = result.ModDirectives["SkyUI"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        directive.Archive.Should().Be("nexus_skyrimspecialedition_1_1");
        directive.Source.Should().Be("interface/iconmenu.swf");
        directive.Destination.Should().Be("interface/iconmenu.swf");
        directive.Hash.Should().Be(hash);
        result.InlineFiles.Should().BeEmpty();
        result.Orphans.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_FileNotInArchive_SmallFile_ProducesInlineDirective()
    {
        var modContent = new byte[] { 1, 2, 3 };
        var hash = HashOf(modContent);

        CreateZip("other.zip", ("unrelated.txt", new byte[] { 9, 9, 9 }));
        WriteModFile("CustomMod", "custom.txt", modContent);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_2_2", "other.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>
            {
                ["CustomMod"] = new[]
                {
                    new ScannedFile
                    {
                        RelativePath = "custom.txt",
                        Hash         = hash,
                        Size         = modContent.Length,
                    },
                },
            },
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives["CustomMod"].Should().HaveCount(1);
        var directive = result.ModDirectives["CustomMod"][0]
            .Should().BeOfType<InlineFileDirective>().Subject;
        directive.Destination.Should().Be("custom.txt");
        result.InlineFiles.Should().HaveCount(1);
        result.InlineFiles[0].Content.Should().Equal(modContent);
        result.InlineFiles[0].Hash.Should().Be(hash);
        result.Orphans.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_FileNotInArchive_LargeFile_ProducesOrphan()
    {
        var largeContent = new byte[3 * 1024 * 1024];   // 3 МБ > 2 МБ
        for (int i = 0; i < largeContent.Length; i++)
            largeContent[i] = (byte)(i & 0xFF);
        var hash = HashOf(largeContent);

        CreateZip("other.zip", ("unrelated.txt", new byte[] { 9 }));
        WriteModFile("BigMod", "large.bin", largeContent);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_3_3", "other.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>
            {
                ["BigMod"] = new[]
                {
                    new ScannedFile
                    {
                        RelativePath = "large.bin",
                        Hash         = hash,
                        Size         = largeContent.Length,
                    },
                },
            },
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives["BigMod"].Should().BeEmpty();
        result.InlineFiles.Should().BeEmpty();
        result.Orphans.Should().HaveCount(1);
        result.Orphans[0].ModName.Should().Be("BigMod");
        result.Orphans[0].RelativePath.Should().Be("large.bin");
    }

    [Fact]
    public async Task Execute_MultipleMods_AllMatched()
    {
        var content1 = new byte[] { 1, 1, 1 };
        var content2 = new byte[] { 2, 2, 2 };
        var hash1 = HashOf(content1);
        var hash2 = HashOf(content2);

        CreateZip("combined.zip",
            ("file1.txt", content1),
            ("file2.txt", content2));

        WriteModFile("ModA", "file1.txt", content1);
        WriteModFile("ModB", "file2.txt", content2);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_4_4", "combined.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>
            {
                ["ModA"] = new[] { new ScannedFile { RelativePath = "file1.txt", Hash = hash1, Size = content1.Length } },
                ["ModB"] = new[] { new ScannedFile { RelativePath = "file2.txt", Hash = hash2, Size = content2.Length } },
            },
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives.Should().HaveCount(2);
        result.ModDirectives["ModA"].Should().HaveCount(1);
        result.ModDirectives["ModB"].Should().HaveCount(1);
        result.Orphans.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_EmptyModScan_ProducesEmptyResult()
    {
        var archiveIndex = new ArchiveIndex
        {
            Resolved = Array.Empty<ArchiveEntry>(),
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>(),
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives.Should().BeEmpty();
        result.InlineFiles.Should().BeEmpty();
        result.Orphans.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_MissingArchiveFile_SkipsGracefully()
    {
        // ArchiveEntry указывает на несуществующий файл
        var fakeEntry = new ArchiveEntry
        {
            Id = "nexus_skyrimspecialedition_99_99",
            Name = "does-not-exist.zip",
            Size = 0,
            Hash = new XxHash64Value(0),
            Sources = new ArchiveSourceRef[]
            {
                new NexusSourceRef { ModId = 99, FileId = 99, Game = "skyrimspecialedition" },
            },
        };

        var content = new byte[] { 1, 2, 3 };
        WriteModFile("Mod", "file.txt", content);

        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { fakeEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>
            {
                ["Mod"] = new[]
                {
                    new ScannedFile
                    {
                        RelativePath = "file.txt",
                        Hash         = HashOf(content),
                        Size         = content.Length,
                    },
                },
            },
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        // Файл не найден в архиве → inline
        result.ModDirectives["Mod"].Should().HaveCount(1);
        result.ModDirectives["Mod"][0].Should().BeOfType<InlineFileDirective>();
        result.InlineFiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task Execute_DuplicateHashInMultipleArchives_FirstWins()
    {
        var content = new byte[] { 5, 5, 5 };
        var hash = HashOf(content);

        // Два архива содержат одинаковый файл
        CreateZip("first.zip", ("file.txt", content));
        CreateZip("second.zip", ("file.txt", content));

        WriteModFile("Mod", "file.txt", content);

        var entry1 = MakeArchiveEntry("nexus_skyrimspecialedition_1_1", "first.zip");
        var entry2 = MakeArchiveEntry("nexus_skyrimspecialedition_2_2", "second.zip");

        var archiveIndex = new ArchiveIndex
        {
            // Порядок важен: first идёт первым
            Resolved = new[] { entry1, entry2 },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>
            {
                ["Mod"] = new[]
                {
                    new ScannedFile
                    {
                        RelativePath = "file.txt",
                        Hash         = hash,
                        Size         = content.Length,
                    },
                },
            },
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        var directive = result.ModDirectives["Mod"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        // Первый архив выиграл
        directive.Archive.Should().Be("nexus_skyrimspecialedition_1_1");
    }

    [Fact]
    public async Task Execute_EmptyArchive_NoDirectives()
    {
        CreateZip("empty.zip");
        var entry = MakeArchiveEntry("nexus_skyrimspecialedition_1_1", "empty.zip");

        var content = new byte[] { 1, 2, 3 };
        WriteModFile("Mod", "file.txt", content);

        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { entry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = new ModScanResult
        {
            Mods = new Dictionary<string, IReadOnlyList<ScannedFile>>
            {
                ["Mod"] = new[]
                {
                    new ScannedFile
                    {
                        RelativePath = "file.txt",
                        Hash         = HashOf(content),
                        Size         = content.Length,
                    },
                },
            },
        };

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        // Файл не найден → inline
        result.ModDirectives["Mod"][0].Should().BeOfType<InlineFileDirective>();
        result.InlineFiles.Should().HaveCount(1);
    }
}
