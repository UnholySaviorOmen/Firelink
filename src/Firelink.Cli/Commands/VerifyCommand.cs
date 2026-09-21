using Firelink.Cli.Settings;
using Firelink.Core;
using Firelink.Install.Verify;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Firelink.Cli.Commands;

/// <summary>
/// `firelink verify &lt;target&gt;` — read-only проверка
/// соответствия инстанса манифесту.
///
/// Exit code:
///   0 — все проверки прошли;
///   1 — есть провалы;
///   2 — исключение (ловит Program.cs);
///   130 — отменено пользователем (Ctrl+C).
/// </summary>
public sealed class VerifyCommand : Command<VerifySettings>
{
    private readonly VerifyPipeline _pipeline;
    private readonly IAnsiConsole _console;

    public VerifyCommand(VerifyPipeline pipeline, IAnsiConsole console)
    {
        _pipeline = pipeline;
        _console = console;
    }

    public override int Execute(CommandContext context, VerifySettings settings)
    {
        var target = Path.GetFullPath(settings.Target);

        _console.MarkupLine("[cyan]Firelink verify[/]");
        _console.MarkupLine($"[grey]Target:[/] {target}");
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
            var report = _pipeline.Execute(target, cts.Token);

            var manifestParse = report.Checks
                .FirstOrDefault(c => c.Name == "Manifest parses");

            if (manifestParse is not null && manifestParse.Passed
                && !string.IsNullOrEmpty(manifestParse.Message))
            {
                _console.MarkupLine($"[grey]Manifest:[/] {manifestParse.Message}");
                _console.WriteLine();
            }

            _console.MarkupLine($"[green]Passed:[/] {report.PassedCount}");
            _console.MarkupLine(
                report.FailedCount == 0
                    ? $"[green]Failed:[/] 0"
                    : $"[red]Failed:[/] {report.FailedCount}");
            _console.WriteLine();

            if (report.FailedCount > 0)
            {
                _console.MarkupLine("[red]Failures:[/]");
                foreach (var f in report.Failures.OrderBy(f => f.Name, StringComparer.Ordinal))
                {
                    var msg = string.IsNullOrEmpty(f.Message) ? "" : " — " + f.Message;
                    _console.MarkupLine($"  [red]×[/] {Markup.Escape(f.Name)}{Markup.Escape(msg)}");
                }
                _console.WriteLine();
            }

            if (settings.Verbose)
            {
                _console.MarkupLine("[grey]All checks:[/]");
                foreach (var c in report.Checks.OrderBy(c => c.Name, StringComparer.Ordinal))
                {
                    var msg = string.IsNullOrEmpty(c.Message) ? "" : " — " + c.Message;
                    if (c.Passed)
                    {
                        _console.MarkupLine(
                            $"  [green]✓[/] {Markup.Escape(c.Name)}{Markup.Escape(msg)}");
                    }
                    else
                    {
                        _console.MarkupLine(
                            $"  [red]×[/] {Markup.Escape(c.Name)}{Markup.Escape(msg)}");
                    }
                }
                _console.WriteLine();
            }

            if (report.IsOk)
            {
                _console.MarkupLine("[green]All checks passed.[/]");
                return 0;
            }

            _console.MarkupLine(
                $"[red]Done: {report.FailedCount} check(s) failed.[/]");
            return 1;
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
