using Firelink.Install.Commands;
using Firelink.Install.Infrastructure;
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

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("firelink-install");
    config.SetApplicationVersion("0.1.0");

    config.AddCommand<InstallCommand>("install")
          .WithDescription("Install a modpack from manifest");

    config.AddCommand<VerifyCommand>("verify")
          .WithDescription("Verify installed instance");

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
