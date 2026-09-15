using Firelink.Install.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Install.Commands;

public sealed class InstallCommand : AsyncCommand<InstallSettings>
{
    private readonly IAnsiConsole _console;

    public InstallCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public override Task<int> ExecuteAsync(CommandContext context, InstallSettings settings)
    {
        _console.MarkupLine("[yellow]install: not implemented yet (v0.2.0).[/]");
        _console.MarkupLine($"[grey]Manifest:[/] {settings.ManifestPath}");
        if (settings.Target is not null)
            _console.MarkupLine($"[grey]Target:[/] {settings.Target}");
        return Task.FromResult(0);
    }
}
