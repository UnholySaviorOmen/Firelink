using Firelink.Install.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Install.Commands;

public sealed class VerifyCommand : Command<VerifySettings>
{
    private readonly IAnsiConsole _console;

    public VerifyCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public override int Execute(CommandContext context, VerifySettings settings)
    {
        _console.MarkupLine("[yellow]verify: not implemented yet (v0.2.0).[/]");
        _console.MarkupLine($"[grey]Target:[/] {settings.Target}");
        return 0;
    }
}
