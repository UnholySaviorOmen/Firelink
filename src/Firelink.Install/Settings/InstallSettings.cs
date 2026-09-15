using System.ComponentModel;
using Spectre.Console.Cli;

namespace Firelink.Install.Settings;

public sealed class InstallSettings : CommandSettings
{
    [CommandArgument(0, "<manifest>")]
    [Description("Path to modlist.json")]
    public required string ManifestPath { get; init; }

    [CommandOption("--target <dir>")]
    [Description("Target directory (default: directory of manifest)")]
    public string? Target { get; init; }
}
