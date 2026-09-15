using FluentAssertions;
using Firelink.Core.Models.Manifest.Sources;
using Firelink.Core.Models.Pack;
using Firelink.Core.Validation;

namespace Firelink.Core.Tests;

public class PackConfigValidatorTests
{
    private static PackConfig MakeValidConfig() => new()
    {
        Meta = new PackMeta
        {
            Name = "Test Pack",
            Version = "1.0.0",
            Author = "tester",
            Game = "skyrimspecialedition",
            GameVersion = "1.6.1170",
        },
        Instance = new PackInstance { Path = "Test Pack" },
        Mo2 = new PackMo2
        {
            Version = "2.5.2",
            Profile = "Default",
            Archive = "Mod.Organizer-2.5.2.7z",
            Source = new GitHubSourceRef
            {
                Repo = "ModOrganizer2/modorganizer",
                Tag = "v2.5.2",
                Asset = "Mod.Organizer-2.5.2.7z",
            },
            Extensions = Array.Empty<string>(),
        },
        StockGame = new PackStockGame { Extras = Array.Empty<string>() },
        ArchiveSources = Array.Empty<PackArchiveSource>(),
    };

    [Fact]
    public void Validate_MinimalValidConfig_Pass()
    {
        PackConfigValidator.Validate(MakeValidConfig()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_FullValidConfig_Pass()
    {
        var config = MakeValidConfig() with
        {
            Instance = new PackInstance { Path = "NordicUI Overhaul" },
            Mo2 = MakeValidConfig().Mo2 with
            {
                Profile = "NordicUI",
                Extensions = new[] { "plugins/fomod.dll", "tools/BethINI/" },
            },
            StockGame = new PackStockGame { Extras = new[] { "skse64_loader.exe" } },
            ArchiveSources = new[]
            {
                new PackArchiveSource
                {
                    Archive = "SomeMod.7z",
                    Sources = new ArchiveSourceRef[]
                    {
                        new MirrorSourceRef
                        {
                            Url = "https://example.com/x.7z",
                            Hash = new Firelink.Core.Models.Hashing.XxHash64Value(0xabc),
                        },
                    },
                },
            },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeTrue(
            $"errors: {string.Join("; ", result.Errors)}");
    }

    [Fact]
    public void Validate_InvalidName_Fails()
    {
        var config = MakeValidConfig() with
        {
            Meta = MakeValidConfig().Meta with { Name = "CON" },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("reserved"));
    }

    [Fact]
    public void Validate_InvalidVersion_Fails()
    {
        var config = MakeValidConfig() with
        {
            Meta = MakeValidConfig().Meta with { Version = "v1.0" },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("semver"));
    }

    [Fact]
    public void Validate_EmptyGame_Fails()
    {
        var config = MakeValidConfig() with
        {
            Meta = MakeValidConfig().Meta with { Game = "" },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("meta.game"));
    }

    [Fact]
    public void Validate_InvalidInstancePath_Fails()
    {
        var config = MakeValidConfig() with
        {
            Instance = new PackInstance { Path = "../outside" },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("instance.path"));
    }

    [Fact]
    public void Validate_InvalidProfileName_Fails()
    {
        var config = MakeValidConfig() with
        {
            Mo2 = MakeValidConfig().Mo2 with { Profile = "CON" },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("mo2.profile"));
    }

    [Fact]
    public void Validate_Mo2ArchiveWithPath_Fails()
    {
        var config = MakeValidConfig() with
        {
            Mo2 = MakeValidConfig().Mo2 with { Archive = "subdir/Mod.7z" },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("mo2.archive"));
    }

    [Fact]
    public void Validate_AbsoluteExtensionPath_Fails()
    {
        var config = MakeValidConfig() with
        {
            Mo2 = MakeValidConfig().Mo2 with
            {
                Extensions = new[] { "C:\\absolute\\path" },
            },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("mo2.extensions[0]"));
    }

    [Fact]
    public void Validate_DuplicateArchiveSources_Fails()
    {
        var config = MakeValidConfig() with
        {
            ArchiveSources = new[]
            {
                new PackArchiveSource
                {
                    Archive = "SomeMod.7z",
                    Sources = new ArchiveSourceRef[]
                    {
                        new GitHubSourceRef { Repo = "a/b", Tag = "v1", Asset = "x.7z" },
                    },
                },
                new PackArchiveSource
                {
                    Archive = "somemod.7z",
                    Sources = new ArchiveSourceRef[]
                    {
                        new GitHubSourceRef { Repo = "a/b", Tag = "v1", Asset = "x.7z" },
                    },
                },
            },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("duplicate"));
    }

    [Fact]
    public void Validate_EmptySources_Fails()
    {
        var config = MakeValidConfig() with
        {
            ArchiveSources = new[]
            {
                new PackArchiveSource
                {
                    Archive = "SomeMod.7z",
                    Sources = Array.Empty<ArchiveSourceRef>(),
                },
            },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least one source"));
    }

    [Fact]
    public void Validate_MultipleErrors_ReportsAll()
    {
        var config = MakeValidConfig() with
        {
            Meta = MakeValidConfig().Meta with { Name = "CON", Version = "bad" },
        };

        var result = PackConfigValidator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
    }
}
