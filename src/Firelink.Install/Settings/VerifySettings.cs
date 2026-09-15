using System.ComponentModel;
using Spectre.Console.Cli;

namespace Firelink.Install.Settings;

public sealed class VerifySettings : CommandSettings
{
    [CommandArgument(0, "<target>")]
    [Description("Target instance directory")]
    public required string Target { get; init; }
}
