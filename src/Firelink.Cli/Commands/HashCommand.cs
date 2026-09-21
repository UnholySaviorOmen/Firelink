using Firelink.Core.Models.Hashing;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Cli.Commands;

public sealed class HashSettings : CommandSettings
{
    [CommandArgument(0, "<file>")]
    public required string FilePath { get; init; }
}

public sealed class HashCommand : Command<HashSettings>
{
    private readonly IAnsiConsole _console;

    public HashCommand(IAnsiConsole console)
    {
        _console = console;
    }

    public override int Execute(CommandContext context, HashSettings settings)
    {
        var path = Path.GetFullPath(settings.FilePath);

        if (!File.Exists(path))
        {
            _console.MarkupLine($"[red]File not found:[/] {path}");
            return 1;
        }

        var info = new FileInfo(path);
        _console.MarkupLine($"[grey]File:[/] {path}");
        _console.MarkupLine($"[grey]Size:[/] {info.Length:N0} bytes");
        _console.WriteLine();

        var hash = XxHash64Value.FromFile(path);

        _console.MarkupLine($"[green]{hash}[/]");
        return 0;
    }
}
