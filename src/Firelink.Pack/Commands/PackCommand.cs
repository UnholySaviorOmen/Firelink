using Firelink.Core.Models.Manifest.Directives;
using Firelink.Pack.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Pack.Commands;

public sealed class PackCommand : AsyncCommand<PackSettings>
{
    private readonly PackPipeline _pipeline;
    private readonly ParallelOptions _parallelOptions;
    private readonly IAnsiConsole _console;

    public PackCommand(
        PackPipeline pipeline,
        ParallelOptions parallelOptions,
        IAnsiConsole console)
    {
        _pipeline = pipeline;
        _parallelOptions = parallelOptions;
        _console = console;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, PackSettings settings)
    {
        var configPath = Path.GetFullPath(settings.ConfigPath);

        _console.MarkupLine($"[cyan]Firelink pack[/]");
        _console.MarkupLine($"[grey]Config:[/] {configPath}");
        _console.WriteLine();

        var result = await _pipeline.ExecuteAsync(
            configPath, _parallelOptions, CancellationToken.None);

        // Подсчёт директив.
        int totalDirectives = 0;
        int fromArchiveCount = 0;
        int inlineCount = 0;
        foreach (var (_, directives) in result.Match.ModDirectives)
        {
            foreach (var d in directives)
            {
                totalDirectives++;
                if (d is FromArchiveDirective) fromArchiveCount++;
                else if (d is InlineFileDirective) inlineCount++;
            }
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Field")
            .AddColumn("Value");

        table.AddRow("Name", result.Config.Meta.Name)
             .AddRow("Version", result.Config.Meta.Version)
             .AddRow("Game", result.Config.Meta.Game)
             .AddRow("Instance", result.Snapshot.InstancePath)
             .AddRow("Mods (all)", result.Snapshot.Modlist.Entries.Count.ToString())
             .AddRow("Plugins", result.Snapshot.Plugins.Entries.Count.ToString())
             .AddRow("Loadorder", result.Snapshot.Loadorder.Plugins.Count.ToString())
             .AddRow("Archives (resolved)", result.ArchiveIndex.Resolved.Count.ToString())
             .AddRow("Archives (unresolved)", result.ArchiveIndex.Unresolved.Count.ToString())
             .AddRow("Mods (scanned)", result.ModScan.TotalMods.ToString())
             .AddRow("Files scanned", result.ModScan.TotalFiles.ToString())
             .AddRow("Directives", totalDirectives.ToString())
             .AddRow("  → FromArchive", fromArchiveCount.ToString())
             .AddRow("  → Inline", inlineCount.ToString())
             .AddRow("Unmatched files", result.Match.Unmatched.Count.ToString())
             .AddRow("meta.ini", result.Match.ModMetas.Count.ToString());

        _console.Write(table);
        _console.MarkupLine("[green]Done (partial — pipeline до Match).[/]");

        return 0;
    }
}
