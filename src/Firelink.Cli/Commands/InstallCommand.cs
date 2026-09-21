using Firelink.Cli.Settings;
using Firelink.Core;
using Firelink.Install;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Cli.Commands;

public sealed class InstallCommand : AsyncCommand<InstallSettings>
{
    private readonly InstallPipeline _pipeline;
    private readonly ParallelOptions _parallelOptions;
    private readonly IAnsiConsole _console;

    public InstallCommand(
        InstallPipeline pipeline,
        ParallelOptions parallelOptions,
        IAnsiConsole console)
    {
        _pipeline = pipeline;
        _parallelOptions = parallelOptions;
        _console = console;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context, InstallSettings settings)
    {
        var input = InstallInputFactory.Create(
            settings.ManifestPath,
            target: settings.Target,
            parallelOptions: _parallelOptions);

        _console.MarkupLine("[cyan]Firelink install[/]");
        _console.MarkupLine($"[grey]Manifest:[/] {input.ManifestPath}");
        if (!string.IsNullOrWhiteSpace(input.Target))
            _console.MarkupLine($"[grey]Target:[/]   {input.Target}");
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
            var output = await _pipeline.ExecuteAsync(input, cts.Token);
            var summary = InstallSummaryBuilder.Build(output);

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Field")
                .AddColumn("Value");

            table.AddRow("Name", summary.Name)
                 .AddRow("Version", summary.Version)
                 .AddRow("Game", summary.Game)
                 .AddRow("Instance", summary.InstancePath)
                 .AddRow("Manifest", summary.ManifestPathInInstance);

            table.AddRow("Archives (already present)",
                summary.ArchivesAlreadyPresent.ToString());
            table.AddRow("Archives (downloaded)",
                summary.ArchivesDownloaded.ToString());
            table.AddRow("Archives (skipped)",
                summary.ArchivesSkipped.ToString());

            table.AddRow("Mods (created)",
                summary.ModsCreated.ToString());
            table.AddRow("Mods (recreated)",
                summary.ModsRecreated.ToString());
            table.AddRow("Mods (skipped)",
                summary.ModsSkipped.ToString());
            table.AddRow("Mods (deleted)",
                summary.ModsDeleted.ToString());

            table.AddRow("meta.ini (written)",
                summary.MetaIniWritten.ToString());
            table.AddRow("meta.ini (deleted)",
                summary.MetaIniDeleted.ToString());

            table.AddRow("MO2 extensions (written)",
                summary.ExtensionsWritten.ToString());
            table.AddRow("MO2 extensions (skipped)",
                summary.ExtensionsSkipped.ToString());
            table.AddRow("Stock Game extras (written)",
                summary.ExtrasWritten.ToString());
            table.AddRow("Stock Game extras (skipped)",
                summary.ExtrasSkipped.ToString());

            table.AddRow("Profile mods",
                summary.ProfileMods.ToString());
            table.AddRow("Profile plugins",
                summary.ProfilePlugins.ToString());
            table.AddRow("Profile loadorder",
                summary.ProfileLoadorder.ToString());

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
