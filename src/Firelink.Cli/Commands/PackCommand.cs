using Firelink.Cli.Settings;
using Firelink.Core;
using Firelink.Pack;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Cli.Commands;

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

    public override async Task<int> ExecuteAsync(
        CommandContext context, PackSettings settings)
    {
        var input = PackInputFactory.Create(
            settings.ConfigPath,
            parallelOptions: _parallelOptions);

        _console.MarkupLine($"[cyan]Firelink pack[/]");
        _console.MarkupLine($"[grey]Config:[/] {input.ConfigPath}");
        _console.WriteLine();

        using var cts = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            _console.MarkupLine("[yellow]Cancellation requested...[/]");
        };
        Console.CancelKeyPress += handler;

        try
        {
            var result = await _pipeline.ExecuteAsync(input, cts.Token);
            var summary = PackSummaryBuilder.Build(result);

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Field")
                .AddColumn("Value");

            table.AddRow("Name", summary.Name)
                 .AddRow("Version", summary.Version)
                 .AddRow("Game", summary.Game)
                 .AddRow("Instance", summary.InstancePath)
                 .AddRow("Mods (all)", summary.ModsTotal.ToString())
                 .AddRow("Plugins", summary.PluginsTotal.ToString())
                 .AddRow("Loadorder", summary.LoadorderTotal.ToString())
                 .AddRow("Archives (resolved)", summary.ArchivesResolved.ToString())
                 .AddRow("Archives (unresolved)", summary.ArchivesUnresolved.ToString())
                 .AddRow("Mods (scanned)", summary.ModsScanned.ToString())
                 .AddRow("Files scanned", summary.FilesScanned.ToString())
                 .AddRow("Directives", summary.DirectivesTotal.ToString())
                 .AddRow("  → FromArchive", summary.DirectivesFromArchive.ToString())
                 .AddRow("Unmatched files", summary.UnmatchedFiles.ToString())
                 .AddRow("meta.ini", summary.MetaIniCount.ToString())
                 .AddRow("Manifest mods", summary.ManifestMods.ToString())
                 .AddRow("Manifest archives", summary.ManifestArchives.ToString())
                 .AddRow("Manifest extensions", summary.ManifestExtensions.ToString())
                 .AddRow("Manifest extras", summary.ManifestExtras.ToString())
                 .AddRow("mo2.archive", "1");

            if (summary.UnmatchedWrittenTo is not null)
                table.AddRow("Unmatched written to", summary.UnmatchedWrittenTo);

            table.AddRow("Manifest written to", summary.ManifestPath);

            _console.Write(table);
            _console.MarkupLine("[green]Done.[/]");

            return 0;
        }
        catch (Exception ex) when (CancellationHelper.IsCancellation(ex))
        {
            _console.MarkupLine("[yellow]Cancelled.[/]");
            return 130;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }
}
