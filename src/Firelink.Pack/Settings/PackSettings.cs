using System.ComponentModel;
using Spectre.Console.Cli;

namespace Firelink.Pack.Settings;

public sealed class PackSettings : CommandSettings
{
    [CommandArgument(0, "<config>")]
    [Description("Path to firelink-pack.json")]
    public required string ConfigPath { get; init; }
}
