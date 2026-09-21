using System.ComponentModel;
using Spectre.Console.Cli;

namespace Firelink.Cli.Settings;

public sealed class VerifySettings : CommandSettings
{
    [CommandArgument(0, "<target>")]
    [Description("Target instance directory to verify")]
    public required string Target { get; init; }

    [CommandOption("-v|--verbose")]
    [Description("Show all checks, including passed ones")]
    [DefaultValue(false)]
    public bool Verbose { get; init; }
}
