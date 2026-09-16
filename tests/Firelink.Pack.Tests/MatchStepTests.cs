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
    private readonly string _outputDir;
    private readonly FileHashCache _hashCache = new();
    private readonly MatchStep _step;

    public MatchStepTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "firelink-match-" + Guid.NewGuid());
        _downloadsDir = Path.Combine(_tempDir, "downloads");
        _modsDir = Path.Combine(_tempDir, "mods");
        _outputDir = Path.Combine(_tempDir, "__Firelink_Output");
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

    // ------------------------------------------------------------------
    //  Хелперы
    // ------------------------------------------------------------------

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

    private void WriteModFile(string modName, string relativePath, string content)
        => WriteModFile(modName, relativePath,
            System.Text.Encoding.UTF8.GetBytes(content));

    private bool OutputFileExists(string modName, string relativePath)
    {
        var fullPath = Path.Combine(_outputDir, "mods", modName,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(fullPath);
    }

    private byte[] ReadOutputFile(string modName, string relativePath)
    {
        var fullPath = Path.Combine(_outputDir, "mods", modName,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllBytes(fullPath);
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
            FirelinkOutputPath = _outputDir,
        };

    private static ModScanResult MakeModScan(
        params (string modName, ScannedFile[] files)[] mods)
    {
        var dict = new Dictionary<string, IReadOnlyList<ScannedFile>>(StringComparer.Ordinal);
        foreach (var (name, files) in mods)
            dict[name] = files;
        return new ModScanResult { Mods = dict };
    }

    private static ScannedFile MakeScannedFile(string path, byte[] content)
        => new()
        {
            RelativePath = path,
            Hash = HashOf(content),
            Size = content.Length,
        };

    // ------------------------------------------------------------------
    //  Базовые тесты
    // ------------------------------------------------------------------

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
        var modScan = MakeModScan(("SkyUI", new[]
        {
            new ScannedFile
            {
                RelativePath = "interface/iconmenu.swf",
                Hash         = hash,
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives["SkyUI"].Should().HaveCount(1);
        var directive = result.ModDirectives["SkyUI"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        directive.Archive.Should().Be("nexus_skyrimspecialedition_1_1");
        directive.Source.Should().Be("interface/iconmenu.swf");
        directive.Destination.Should().Be("interface/iconmenu.swf");
        directive.Hash.Should().Be(hash);
        result.Unmatched.Should().BeEmpty();
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
        var modScan = MakeModScan(
            ("ModA", new[] { new ScannedFile { RelativePath = "file1.txt", Hash = hash1, Size = content1.Length } }),
            ("ModB", new[] { new ScannedFile { RelativePath = "file2.txt", Hash = hash2, Size = content2.Length } }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives.Should().HaveCount(2);
        result.ModDirectives["ModA"].Should().HaveCount(1);
        result.ModDirectives["ModB"].Should().HaveCount(1);
        result.Unmatched.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_EmptyModScan_ProducesEmptyResult()
    {
        var archiveIndex = new ArchiveIndex
        {
            Resolved = Array.Empty<ArchiveEntry>(),
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan();

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives.Should().BeEmpty();
        result.Unmatched.Should().BeEmpty();
        result.ModMetas.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_MissingArchiveFile_SkipsGracefully()
    {
        // ArchiveEntry указывает на несуществующий файл.
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
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "file.txt",
                Hash         = HashOf(content),
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        // Файл не найден → Unmatched, выгружен в __Firelink_Output.
        result.ModDirectives["Mod"].Should().BeEmpty();
        result.Unmatched.Should().HaveCount(1);
        result.Unmatched[0].ModName.Should().Be("Mod");
        result.Unmatched[0].RelativePath.Should().Be("file.txt");
        OutputFileExists("Mod", "file.txt").Should().BeTrue();
    }

    [Fact]
    public async Task Execute_DuplicateHashInMultipleArchives_DeterministicByArchiveId()
    {
        var content = new byte[] { 5, 5, 5 };
        var hash = HashOf(content);

        CreateZip("first.zip", ("file.txt", content));
        CreateZip("second.zip", ("file.txt", content));

        WriteModFile("Mod", "file.txt", content);

        var entry1 = MakeArchiveEntry("nexus_skyrimspecialedition_1_1", "first.zip");
        var entry2 = MakeArchiveEntry("nexus_skyrimspecialedition_2_2", "second.zip");

        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { entry1, entry2 },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "file.txt",
                Hash         = hash,
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        var directive = result.ModDirectives["Mod"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        // Детерминизм: первый по алфавиту archiveId.
        directive.Archive.Should().Be("nexus_skyrimspecialedition_1_1");
    }

    // ------------------------------------------------------------------
    //  Матчинг по (hash, path) и по hash
    // ------------------------------------------------------------------

    [Fact]
    public async Task Match_ByHashDifferentPath_ProducesFromArchive()
    {
        // enbseries.ini.mohidden в моде, enbseries.ini в архиве. Хеши совпадают.
        var content = new byte[] { 10, 20, 30 };
        var hash = HashOf(content);

        CreateZip("enb.zip", ("enbseries.ini", content));
        WriteModFile("Mod", "enbseries.ini.mohidden", content);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_7_7", "enb.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "enbseries.ini.mohidden",
                Hash         = hash,
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        var directive = result.ModDirectives["Mod"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        directive.Source.Should().Be("enbseries.ini");
        directive.Destination.Should().Be("enbseries.ini.mohidden");
        result.Unmatched.Should().BeEmpty();
    }

    [Fact]
    public async Task Match_ByHashDifferentPath_ReverseDirection()
    {
        // Наоборот: в моде enbseries.ini, в архиве enbseries.ini.mohidden.
        var content = new byte[] { 10, 20, 30 };
        var hash = HashOf(content);

        CreateZip("enb.zip", ("enbseries.ini.mohidden", content));
        WriteModFile("Mod", "enbseries.ini", content);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_8_8", "enb.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "enbseries.ini",
                Hash         = hash,
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        var directive = result.ModDirectives["Mod"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        directive.Source.Should().Be("enbseries.ini.mohidden");
        directive.Destination.Should().Be("enbseries.ini");
        result.Unmatched.Should().BeEmpty();
    }

    [Fact]
    public async Task Match_ExactWinsOverByHash()
    {
        // Два архива: в одном точное совпадение, в другом совпадение по хешу.
        // Ожидание: берётся точное, source == destination.
        var content = new byte[] { 100, 101, 102 };
        var hash = HashOf(content);

        CreateZip("a-exact.zip", ("enbseries.ini", content));
        CreateZip("z-byhash.zip", ("enbseries.ini.mohidden", content));

        WriteModFile("Mod", "enbseries.ini", content);

        // a-exact идёт по алфавиту первым, но это не важно — точное совпадение
        // должно победить, даже если бы точный был вторым.
        var exactEntry = MakeArchiveEntry("nexus_skyrimspecialedition_1_1", "a-exact.zip");
        var byHashEntry = MakeArchiveEntry("nexus_skyrimspecialedition_2_2", "z-byhash.zip");

        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { exactEntry, byHashEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "enbseries.ini",
                Hash         = hash,
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        var directive = result.ModDirectives["Mod"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        directive.Archive.Should().Be("nexus_skyrimspecialedition_1_1");
        directive.Source.Should().Be("enbseries.ini");
        directive.Destination.Should().Be("enbseries.ini");
    }

    [Fact]
    public async Task Match_MohiddenFolder_Preserved()
    {
        // Папка meshes.mohidden/ в моде. Файлы внутри — обычные.
        // Хеш совпадает с файлом из meshes/ в архиве.
        var content = new byte[] { 7, 7, 7 };
        var hash = HashOf(content);

        CreateZip("meshes.zip", ("meshes/whatever.nif", content));
        WriteModFile("Mod", "meshes.mohidden/whatever.nif", content);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_9_9", "meshes.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "meshes.mohidden/whatever.nif",
                Hash         = hash,
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        var directive = result.ModDirectives["Mod"][0].Should().BeOfType<FromArchiveDirective>().Subject;
        directive.Source.Should().Be("meshes/whatever.nif");
        directive.Destination.Should().Be("meshes.mohidden/whatever.nif");
    }

    // ------------------------------------------------------------------
    //  __Firelink_Output
    // ------------------------------------------------------------------

    [Fact]
    public async Task Unmatched_WrittenToFirelinkOutput_ContentPreserved()
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
        var modScan = MakeModScan(("CustomMod", new[]
        {
            new ScannedFile
            {
                RelativePath = "custom.txt",
                Hash         = hash,
                Size         = modContent.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives["CustomMod"].Should().BeEmpty();
        result.Unmatched.Should().HaveCount(1);
        result.Unmatched[0].ModName.Should().Be("CustomMod");
        result.Unmatched[0].RelativePath.Should().Be("custom.txt");
        result.Unmatched[0].Size.Should().Be(modContent.Length);

        OutputFileExists("CustomMod", "custom.txt").Should().BeTrue();
        ReadOutputFile("CustomMod", "custom.txt").Should().Equal(modContent);
    }

    [Fact]
    public async Task Unmatched_PreservesDirectoryStructure()
    {
        var content = new byte[] { 4, 4, 4 };
        CreateZip("empty-ish.zip", ("something-else.txt", new byte[] { 0 }));
        WriteModFile("Mod", "SKSE/Plugins/config.ini", content);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_3_3", "empty-ish.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "SKSE/Plugins/config.ini",
                Hash         = HashOf(content),
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.Unmatched.Should().HaveCount(1);
        OutputFileExists("Mod", "SKSE/Plugins/config.ini").Should().BeTrue();
        ReadOutputFile("Mod", "SKSE/Plugins/config.ini").Should().Equal(content);
    }

    [Fact]
    public async Task Unmatched_LargeFile_StillWritten()
    {
        // Раньше >2МБ шло в Orphans. Теперь — всё в __Firelink_Output.
        var largeContent = new byte[3 * 1024 * 1024];
        for (int i = 0; i < largeContent.Length; i++)
            largeContent[i] = (byte)(i & 0xFF);

        CreateZip("other.zip", ("unrelated.txt", new byte[] { 9 }));
        WriteModFile("BigMod", "large.bin", largeContent);

        var archiveEntry = MakeArchiveEntry("nexus_skyrimspecialedition_3_3", "other.zip");
        var archiveIndex = new ArchiveIndex
        {
            Resolved = new[] { archiveEntry },
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("BigMod", new[]
        {
            new ScannedFile
            {
                RelativePath = "large.bin",
                Hash         = HashOf(largeContent),
                Size         = largeContent.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.Unmatched.Should().HaveCount(1);
        OutputFileExists("BigMod", "large.bin").Should().BeTrue();
    }

    [Fact]
    public async Task FirelinkOutput_CleanedBeforeRun()
    {
        // Кладём мусор в __Firelink_Output/mods/Garbage/...
        var garbageDir = Path.Combine(_outputDir, "mods", "Garbage");
        Directory.CreateDirectory(garbageDir);
        File.WriteAllText(Path.Combine(garbageDir, "old.txt"), "old");

        var archiveIndex = new ArchiveIndex
        {
            Resolved = Array.Empty<ArchiveEntry>(),
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan();

        await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        File.Exists(Path.Combine(garbageDir, "old.txt")).Should().BeFalse();
        Directory.Exists(garbageDir).Should().BeFalse();
        Directory.Exists(Path.Combine(_outputDir, "mods")).Should().BeTrue();
    }

    // ------------------------------------------------------------------
    //  meta.ini
    // ------------------------------------------------------------------

    [Fact]
    public async Task MetaIni_AtRoot_GoesToModMetas_NotToUnmatched()
    {
        var metaContent =
            "[General]\r\n" +
            "gameName=Skyrim Special Edition\r\n" +
            "gameID=skyrimspecialedition\r\n" +
            "modID=32349\r\n" +
            "fileID=795423\r\n" +
            "version=1.7.0\r\n" +
            "notes=my note\r\n";

        WriteModFile("Actor Limit Fix", "meta.ini", metaContent);

        var archiveIndex = new ArchiveIndex
        {
            Resolved = Array.Empty<ArchiveEntry>(),
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Actor Limit Fix", new[]
        {
            new ScannedFile
            {
                RelativePath = "meta.ini",
                Hash         = HashOf(System.Text.Encoding.UTF8.GetBytes(metaContent)),
                Size         = System.Text.Encoding.UTF8.GetByteCount(metaContent),
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModDirectives["Actor Limit Fix"].Should().BeEmpty();
        result.Unmatched.Should().BeEmpty();
        result.ModMetas.Should().ContainKey("Actor Limit Fix");
        var meta = result.ModMetas["Actor Limit Fix"];
        meta.ModId.Should().Be(32349);
        meta.FileId.Should().Be(795423);
        meta.Version.Should().Be("1.7.0");
        meta.Notes.Should().Be("my note");

        // Не выгружен в __Firelink_Output.
        OutputFileExists("Actor Limit Fix", "meta.ini").Should().BeFalse();
    }

    [Fact]
    public async Task MetaIni_InSubfolder_TreatedAsRegularFile()
    {
        // fomod/meta.ini — это не корневой meta.ini. Идёт как обычный файл.
        var content = new byte[] { 1, 2, 3 };

        WriteModFile("Mod", "fomod/meta.ini", content);

        var archiveIndex = new ArchiveIndex
        {
            Resolved = Array.Empty<ArchiveEntry>(),
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "fomod/meta.ini",
                Hash         = HashOf(content),
                Size         = content.Length,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModMetas.Should().BeEmpty();
        result.Unmatched.Should().HaveCount(1);
        result.Unmatched[0].RelativePath.Should().Be("fomod/meta.ini");
        OutputFileExists("Mod", "fomod/meta.ini").Should().BeTrue();
    }

    [Fact]
    public async Task ModMetas_MultipleMods_OnlyThoseWithMetaIni()
    {
        var meta1 = "[General]\r\nmodID=1\r\nfileID=10\r\n";
        var meta2 = "[General]\r\nmodID=2\r\nfileID=20\r\n";

        WriteModFile("ModA", "meta.ini", meta1);
        WriteModFile("ModB", "meta.ini", meta2);
        WriteModFile("ModC", "readme.txt", "hello");

        var archiveIndex = new ArchiveIndex
        {
            Resolved = Array.Empty<ArchiveEntry>(),
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };

        ScannedFile MetaScanned(string path, string content) => new()
        {
            RelativePath = path,
            Hash = HashOf(System.Text.Encoding.UTF8.GetBytes(content)),
            Size = System.Text.Encoding.UTF8.GetByteCount(content),
        };

        var modScan = MakeModScan(
            ("ModA", new[] { MetaScanned("meta.ini", meta1) }),
            ("ModB", new[] { MetaScanned("meta.ini", meta2) }),
            ("ModC", new[] { MetaScanned("readme.txt", "hello") }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModMetas.Should().HaveCount(2);
        result.ModMetas.Should().ContainKey("ModA");
        result.ModMetas.Should().ContainKey("ModB");
        result.ModMetas.Should().NotContainKey("ModC");

        // ModC/readme.txt — обычный unmatched.
        result.Unmatched.Should().HaveCount(1);
        result.Unmatched[0].ModName.Should().Be("ModC");
    }

    [Fact]
    public async Task ModMetas_MetaIniEmptyFile_IsEmptyButPresent()
    {
        WriteModFile("Mod", "meta.ini", "");

        var archiveIndex = new ArchiveIndex
        {
            Resolved = Array.Empty<ArchiveEntry>(),
            Unresolved = Array.Empty<UnresolvedArchive>(),
        };
        var modScan = MakeModScan(("Mod", new[]
        {
            new ScannedFile
            {
                RelativePath = "meta.ini",
                Hash         = HashOf(Array.Empty<byte>()),
                Size         = 0,
            },
        }));

        var result = await _step.ExecuteAsync(MakeInput(archiveIndex, modScan), CancellationToken.None);

        result.ModMetas.Should().ContainKey("Mod");
        result.ModMetas["Mod"].IsEmpty.Should().BeTrue();
        result.Unmatched.Should().BeEmpty();
        OutputFileExists("Mod", "meta.ini").Should().BeFalse();
    }
}
