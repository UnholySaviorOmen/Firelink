using Firelink.Core.Archives;
using Firelink.Core.Archives.Extraction;
using Firelink.Pack;
using Firelink.Pack.Commands;
using Firelink.Pack.Infrastructure;
using Firelink.Pack.Steps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddSimpleConsole(opts =>
    {
        opts.SingleLine = true;
        opts.TimestampFormat = "HH:mm:ss ";
        opts.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
    });
    builder.SetMinimumLevel(LogLevel.Information);
});

services.AddSingleton<IAnsiConsole>(AnsiConsole.Console);

services.AddSingleton(new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
});

services.AddSingleton<FileHashCache>();
services.AddSingleton<IArchiveExtractor, SevenZipExtractor>();

services.AddSingleton<ReadConfigStep>();
services.AddSingleton<ReadInstanceStep>();
services.AddSingleton<IndexArchivesStep>();
services.AddSingleton<ScanModsStep>();
services.AddSingleton<MatchStep>();

services.AddSingleton<PackPipeline>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("firelink-pack");
    config.SetApplicationVersion("0.1.0");

    config.AddCommand<PackCommand>("pack")
          .WithDescription("Create a manifest from an MO2 instance");

    config.AddCommand<HashCommand>("hash")
          .WithDescription("Compute xxHash64 of a file (format: xxh64:hex)");

    config.AddCommand<DoctorCommand>("doctor")
          .WithDescription("Diagnose environment");
});

try
{
    return await app.RunAsync(args);
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]ERROR:[/] {ex.Message}");
    if (ex.InnerException is not null)
        AnsiConsole.MarkupLine($"[red]  →[/] {ex.InnerException.Message}");
    return 2;
}
