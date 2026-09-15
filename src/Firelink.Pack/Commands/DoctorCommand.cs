using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Pack.Commands;

public sealed class DoctorSettings : CommandSettings { }

public sealed class DoctorCommand : Command<DoctorSettings>
{
    private readonly IAnsiConsole _console;

    public DoctorCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public override int Execute(CommandContext context, DoctorSettings settings)
    {
        _console.MarkupLine("[cyan]Firelink doctor[/]");
        _console.MarkupLine($"[grey]OS:[/] {Environment.OSVersion}");
        _console.MarkupLine($"[grey]Runtime:[/] {Environment.Version}");
        _console.MarkupLine($"[grey]64-bit:[/] {Environment.Is64BitProcess}");
        _console.MarkupLine($"[grey]CPU:[/] {Environment.ProcessorCount} cores");
        _console.MarkupLine("[yellow]Doctor is a stub — more checks later.[/]");
        return 0;
    }
}
