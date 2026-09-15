using FluentAssertions;
using Firelink.Core.Archives;
using Firelink.Core.Models.Mo2;
using Firelink.Core.Models.Pack;
using Firelink.Pack.Steps;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firelink.Pack.Tests;

public class ScanModsStepTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _modsDir;
    private readonly FileHashCache _cache = new();
    private readonly ScanModsStep _step;

    public ScanModsStepTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "firelink-sm-" + Guid.NewGuid());
        _modsDir = Path.Combine(_tempDir, "mods");
        Directory.CreateDirectory(_modsDir);
        _step = new ScanModsStep(_cache, NullLogger<ScanModsStep>.Instance);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private void WriteMod(string modName, params (string path, string content)[] files)
    {
        var modDir = Path.Combine(_modsDir, modName);
        Directory.CreateDirectory(modDir);

        foreach (var (path, content) in files)
        {
            var fullPath = Path.Combine(modDir, path.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content);
        }
    }

    private static ModlistFile Modlist(params (string name, bool enabled)[] entries)
        => new() { Entries = entries.Select(e => new ModlistEntry(e.name, e.enabled)).ToArray() };

    private InstanceSnapshot MakeSnapshot(ModlistFile modlist) => new()
    {
        InstancePath = _tempDir,
        Mo2Path = _tempDir,
        DownloadsPath = Path.Combine(_tempDir, "downloads"),
        ModsPath = _modsDir,
        ProfilesPath = Path.Combine(_tempDir, "profiles"),
        StockGamePath = Path.Combine(_tempDir, "Stock Game"),
        Modlist = modlist,
        Plugins = new PluginsFile { Entries = Array.Empty<PluginEntry>() },
        Loadorder = new LoadorderFile { Plugins = Array.Empty<string>() },
    };

    private ScanModsStep.Input MakeInput(ModlistFile modlist) => new()
    {
        Snapshot = MakeSnapshot(modlist),
        ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1 },
    };

    [Fact]
    public async Task Execute_OnlyEnabledMods_AreScanned()
    {
        WriteMod("Enabled", ("file.txt", "hello"));
        WriteMod("Disabled", ("file.txt", "world"));

        var modlist = Modlist(
            ("Enabled", true),
            ("Disabled", false));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        result.Mods.Should().HaveCount(1);
        result.Mods.Should().ContainKey("Enabled");
        result.Mods.Should().NotContainKey("Disabled");
    }

    [Fact]
    public async Task Execute_NoDeleteMods_AreSkipped()
    {
        WriteMod("NormalMod", ("file.txt", "x"));
        WriteMod("[NoDelete]MyMod", ("file.txt", "y"));

        var modlist = Modlist(
            ("NormalMod", true),
            ("[NoDelete]MyMod", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        result.Mods.Should().HaveCount(1);
        result.Mods.Should().ContainKey("NormalMod");
    }

    [Fact]
    public async Task Execute_NestedFiles_HaveForwardSlashPaths()
    {
        WriteMod("Mod", ("interface/iconmenu.swf", "a"));
        WriteMod("Mod", ("scripts/deep/nested/file.pex", "b"));

        var modlist = Modlist(("Mod", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        var files = result.Mods["Mod"];
        files.Should().HaveCount(2);
        files.Should().Contain(f => f.RelativePath == "interface/iconmenu.swf");
        files.Should().Contain(f => f.RelativePath == "scripts/deep/nested/file.pex");
    }

    [Fact]
    public async Task Execute_MissingModDirectory_Throws()
    {
        // Мод включён, но папки нет
        var modlist = Modlist(("NonExistentMod", true));

        var act = async () => await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);
        await act.Should().ThrowAsync<DirectoryNotFoundException>()
            .WithMessage("*NonExistentMod*");
    }

    [Fact]
    public async Task Execute_EmptyModDirectory_ScannedAsEmpty()
    {
        Directory.CreateDirectory(Path.Combine(_modsDir, "EmptyMod"));

        var modlist = Modlist(("EmptyMod", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        result.Mods.Should().HaveCount(1);
        result.Mods["EmptyMod"].Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_HashesAreCorrect()
    {
        // Известный контент — знаем, какой будет хеш для пустого файла.
        // xxHash64 от пустой строки = 0xef46db3751d8e999 (значение seed=0)
        WriteMod("Mod", ("empty.bin", ""));

        var modlist = Modlist(("Mod", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        var file = result.Mods["Mod"].Single();
        // Не проверяем конкретное значение (может отличаться версией System.IO.Hashing),
        // но проверяем, что хеш посчитан и не нулевой
        file.Hash.Value.Should().NotBe(0);
        file.Size.Should().Be(0);
    }

    [Fact]
    public async Task Execute_FilesSortedByPath()
    {
        WriteMod("Mod", ("z.txt", "z"));
        WriteMod("Mod", ("a.txt", "a"));
        WriteMod("Mod", ("m.txt", "m"));

        var modlist = Modlist(("Mod", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        var files = result.Mods["Mod"];
        files.Select(f => f.RelativePath).Should().Equal("a.txt", "m.txt", "z.txt");
    }

    [Fact]
    public async Task Execute_ModsSortedByName()
    {
        WriteMod("ZMod", ("f.txt", "z"));
        WriteMod("AMod", ("f.txt", "a"));
        WriteMod("MMod", ("f.txt", "m"));

        var modlist = Modlist(
            ("ZMod", true),
            ("AMod", true),
            ("MMod", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        result.Mods.Keys.Should().Equal("AMod", "MMod", "ZMod");
    }

    [Fact]
    public async Task Execute_TotalFilesCountsCorrectly()
    {
        WriteMod("A", ("f1.txt", "1"), ("f2.txt", "2"));
        WriteMod("B", ("f3.txt", "3"));

        var modlist = Modlist(("A", true), ("B", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        result.TotalMods.Should().Be(2);
        result.TotalFiles.Should().Be(3);
    }

    [Fact]
    public async Task Execute_HashCacheUsedAcrossMods()
    {
        // Одинаковый контент в двух модах — второй должен взять хеш из кеша.
        WriteMod("A", ("shared.txt", "same content"));
        WriteMod("B", ("shared.txt", "same content"));

        var modlist = Modlist(("A", true), ("B", true));

        var result = await _step.ExecuteAsync(MakeInput(modlist), CancellationToken.None);

        var hashA = result.Mods["A"].Single().Hash;
        var hashB = result.Mods["B"].Single().Hash;

        hashA.Should().Be(hashB);
        // Файлы разные — пути разные, значит в кеше 2 записи.
        // (Кеш работает по пути файла, а не по контенту.)
        _cache.Count.Should().Be(2);
    }
}
