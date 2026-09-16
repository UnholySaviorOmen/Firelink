using Firelink.Core.Models.Pack;
using Firelink.Platform.MO2.Readers;
using FluentAssertions;

namespace Firelink.Platform.MO2.Tests;

public class MetaIniReaderTests
{
    [Fact]
    public void Parse_FullGeneralSection_ReadsAllFields()
    {
        var lines = new[]
        {
            "[General]",
            "gameName=Skyrim Special Edition",
            "gameID=skyrimspecialedition",
            "modID=32349",
            "fileID=795423",
            "version=1.7.0",
            "category=0",
            "repository=Nexus",
            "url=https://www.nexusmods.com/skyrimspecialedition/mods/32349",
            "comments=some comment",
            "notes=my author note",
        };

        var meta = MetaIniReader.Parse(lines);

        meta.GameName.Should().Be("Skyrim Special Edition");
        meta.GameId.Should().Be("skyrimspecialedition");
        meta.ModId.Should().Be(32349);
        meta.FileId.Should().Be(795423);
        meta.Version.Should().Be("1.7.0");
        meta.Category.Should().Be(0);
        meta.Repository.Should().Be("Nexus");
        meta.Url.Should().Be("https://www.nexusmods.com/skyrimspecialedition/mods/32349");
        meta.Comments.Should().Be("some comment");
        meta.Notes.Should().Be("my author note");
        meta.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Parse_PartialGeneralSection_LeavesOthersNull()
    {
        var lines = new[]
        {
            "[General]",
            "modID=1",
            "version=2.0",
        };

        var meta = MetaIniReader.Parse(lines);

        meta.ModId.Should().Be(1);
        meta.Version.Should().Be("2.0");
        meta.FileId.Should().BeNull();
        meta.GameName.Should().BeNull();
        meta.Category.Should().BeNull();
        meta.Repository.Should().BeNull();
        meta.Url.Should().BeNull();
        meta.Comments.Should().BeNull();
        meta.Notes.Should().BeNull();
    }

    [Fact]
    public void Parse_IgnoresInstalledFilesSection()
    {
        var lines = new[]
        {
            "[General]",
            "modID=1",
            "fileID=2",
            "version=1.0",
            "",
            "[installedFiles]",
            "1\\SKSE\\Plugins\\Foo.dll=DEADBEEF",
            "2\\SKSE\\Plugins\\Foo.pdb=FEEDFACE",
        };

        var meta = MetaIniReader.Parse(lines);

        meta.ModId.Should().Be(1);
        meta.FileId.Should().Be(2);
        meta.Version.Should().Be("1.0");
    }

    [Fact]
    public void Parse_WithoutGeneralSection_ReturnsEmpty()
    {
        var lines = new[]
        {
            "[installedFiles]",
            "1\\foo.dll=DEADBEEF",
        };

        var meta = MetaIniReader.Parse(lines);

        meta.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsEmpty()
    {
        var meta = MetaIniReader.Parse(Array.Empty<string>());
        meta.IsEmpty.Should().BeTrue();
        meta.Should().BeEquivalentTo(ModMeta.Empty);
    }

    [Fact]
    public void Parse_WithBom_Succeeds()
    {
        var lines = new[]
        {
            "\uFEFF[General]",
            "modID=1",
            "fileID=2",
        };

        var meta = MetaIniReader.Parse(lines);
        meta.ModId.Should().Be(1);
        meta.FileId.Should().Be(2);
    }

    [Fact]
    public void Parse_SectionNameCaseInsensitive()
    {
        var lines = new[]
        {
            "[GENERAL]",
            "modID=1",
        };

        var meta = MetaIniReader.Parse(lines);
        meta.ModId.Should().Be(1);
    }

    [Fact]
    public void Parse_KeysCaseSensitive()
    {
        // modid вместо modID — не наш ключ.
        var lines = new[]
        {
            "[General]",
            "modid=1",
            "fileid=2",
        };

        var meta = MetaIniReader.Parse(lines);
        meta.ModId.Should().BeNull();
        meta.FileId.Should().BeNull();
    }

    [Fact]
    public void Parse_NonNumericModId_LeavesNull()
    {
        var lines = new[]
        {
            "[General]",
            "modID=not-a-number",
            "fileID=2",
        };

        var meta = MetaIniReader.Parse(lines);
        meta.ModId.Should().BeNull();
        meta.FileId.Should().Be(2);
    }

    [Fact]
    public void Parse_NonNumericCategory_LeavesNull()
    {
        var lines = new[]
        {
            "[General]",
            "category=abc",
            "modID=1",
        };

        var meta = MetaIniReader.Parse(lines);
        meta.Category.Should().BeNull();
        meta.ModId.Should().Be(1);
    }

    [Fact]
    public void Parse_WithCommentsAndEmptyLines_IgnoresThem()
    {
        var lines = new[]
        {
            "; comment",
            "# another",
            "",
            "[General]",
            "   ",
            "modID=1",
            "; another comment",
            "fileID=2",
        };

        var meta = MetaIniReader.Parse(lines);
        meta.ModId.Should().Be(1);
        meta.FileId.Should().Be(2);
    }

    [Fact]
    public void Parse_DuplicateKeys_FirstWins()
    {
        var lines = new[]
        {
            "[General]",
            "modID=1",
            "modID=2",
        };

        var meta = MetaIniReader.Parse(lines);
        meta.ModId.Should().Be(1);
    }

    [Fact]
    public void TryRead_NonExistentFile_ReturnsNull()
    {
        var meta = MetaIniReader.TryRead(@"C:\does\not\exist\meta.ini");
        meta.Should().BeNull();
    }

    [Fact]
    public void TryRead_RealFile_ParsesCorrectly()
    {
        var tmp = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tmp,
                "\uFEFF[General]\r\n" +
                "gameName=Skyrim Special Edition\r\n" +
                "gameID=skyrimspecialedition\r\n" +
                "modID=32349\r\n" +
                "fileID=795423\r\n" +
                "version=1.7.0\r\n" +
                "\r\n" +
                "[installedFiles]\r\n" +
                "1\\SKSE\\Plugins\\ActorLimitFix.dll=DEADBEEF\r\n");

            var meta = MetaIniReader.TryRead(tmp);

            meta.Should().NotBeNull();
            meta!.ModId.Should().Be(32349);
            meta.FileId.Should().Be(795423);
            meta.Version.Should().Be("1.7.0");
            meta.GameId.Should().Be("skyrimspecialedition");
        }
        finally
        {
            File.Delete(tmp);
        }
    }

    [Fact]
    public void Empty_IsEmpty_True()
    {
        ModMeta.Empty.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WithOnlyGameName_False()
    {
        var m = new ModMeta { GameName = "Skyrim" };
        m.IsEmpty.Should().BeFalse();
    }
}
