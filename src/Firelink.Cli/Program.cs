using System.Reflection;
using Firelink.Core;
using Firelink.Cli.Commands;
using Firelink.Cli.Infrastructure;
using Firelink.Install;
using Firelink.Pack;
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

// --- Pack + Install ---
// Всё, что нужно pipeline-ам, регистрируется здесь.
// FileHashCache и IArchiveExtractor — общие, TryAddSingleton внутри.
services.AddFirelinkPack();
services.AddFirelinkInstall();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("firelink");
    config.SetApplicationVersion(GetApplicationVersion());

    // Spectre по умолчанию сам обрабатывает CommandParseException и печатает
    // красиво отформатированное сообщение, но без нашего hint про кавычки.
    // PropagateExceptions отдаёт исключение нам — мы печатаем свой формат.
    config.PropagateExceptions();

    config.AddCommand<PackCommand>("pack")
          .WithDescription("Create a manifest from an MO2 instance");

    config.AddCommand<InstallCommand>("install")
          .WithDescription("Install a modpack from manifest");

    config.AddCommand<VerifyCommand>("verify")
          .WithDescription("Verify installed instance");

    config.AddCommand<HashCommand>("hash")
          .WithDescription("Compute xxHash64 of a file (format: xxh64:hex)");

    config.AddCommand<DoctorCommand>("doctor")
          .WithDescription("Diagnose environment");
});

try
{
    return await app.RunAsync(args);
}
catch (CommandParseException ex)
{
    // Spectre не смог разобрать argv. Чаще всего это пробелы в пути
    // без кавычек: пользователь написал
    //     firelink install C:\My Packs\modlist.json
    // и Windows отдал нам два аргумента: "C:\My" и "Packs\modlist.json".
    AnsiConsole.MarkupLine($"[red]CLI error:[/] {Markup.Escape(ex.Message)}");

    if (args.Any(a => a.Contains(' ')))
    {
        // Пробелы уже в argv — значит проблема не в кавычках, а в самой
        // команде. Hint не нужен.
    }
    else
    {
        AnsiConsole.MarkupLine(
            "[yellow]Hint:[/] paths with spaces must be quoted:");
        AnsiConsole.MarkupLine(
            "[grey]      firelink install \"C:\\My Packs\\modlist.json\"[/]");
    }

    return 2;
}
catch (Exception ex) when (CancellationHelper.IsCancellation(ex))
{
    // Ctrl+C. Сюда попадаем, если команда не успела поймать отмену
    // (или Spectre обернул её в AggregateException).
    AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
    return 130;
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]ERROR:[/] {ex.Message}");
    if (ex.InnerException is not null)
        AnsiConsole.MarkupLine($"[red]  →[/] {ex.InnerException.Message}");
    return 2;
}

// ---------------------------------------------------------------------
//  Helpers
// ---------------------------------------------------------------------

/// <summary>
/// Версия из entry assembly. Значение приходит из Directory.Build.props
/// (VersionPrefix) через AssemblyInformationalVersionAttribute.
///
/// Fallback "0.0.0" — только если атрибута нет вообще (теоретически
/// невозможно при GenerateAssemblyInfo=true, который .NET SDK ставит
/// по умолчанию).
/// </summary>
static string GetApplicationVersion()
{
    var informational = typeof(Program).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion;

    if (!string.IsNullOrWhiteSpace(informational))
    {
        // На случай, если SourceRevisionId всё-таки просочится
        // (IncludeSourceRevisionInInformationalVersion=false стоит
        // в Directory.Build.props, но подстрахуемся): "0.1.0+abc123"
        // → "0.1.0".
        var plus = informational.IndexOf('+');
        return plus >= 0 ? informational[..plus] : informational;
    }

    return "0.0.0";
}
