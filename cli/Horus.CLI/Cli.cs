using Horus.Commands;
using Horus.Core;
using Horus.Core.Services;
using Horus.Shared;
using Horus.Shared.Enums;
using Spectre.Console;

namespace Horus;

public class Cli
{
    #region Constructors

    public Cli()
    {
        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new FsService(),
            new GitService(),
            new GpgService(),
            new SettingsService()
        ));
    }

    #endregion

    #region Public methods

    public void Run(string[] args)
    {
        SetAppLanguage();

        var (_, error) = AppService.Instance.Verify();
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[red]{error.Message}[/]");
            return;
        }

        var lstArgs = args.ToList();

        lstArgs = HandleAliases(lstArgs);

        var commandArgs = lstArgs.Skip(1).ToList();
        switch (lstArgs[0])
        {
            case "interactive":
            case "interact":
                InteractiveMode();
                break;

            case "init":
                new Init().Run(commandArgs);
                break;

            case "ls":
            case "list":
                new List().Run(commandArgs);
                break;

            case "show":
                new Show().Run(commandArgs);
                break;

            case "insert":
            case "add":
                new Insert().Run(commandArgs);
                break;

            case "edit":
                new Edit().Run(commandArgs);
                break;

            case "grep":
            case "find":
            case "search":
                new Search().Run(commandArgs);
                break;

            case "generate":
                new Generate().Run(commandArgs);
                break;

            case "delete":
            case "remove":
                new Delete().Run(commandArgs);
                break;

            case "rename":
            case "move":
                new Rename().Run(commandArgs);
                break;

            case "help":
                new Help().Run(commandArgs);
                break;

            case "version":
                new ShowVersion().Run(commandArgs);
                break;

            case "git":
                new Git().Run(commandArgs);
                break;

            case "cc":
                new ClearClipboard().Run(commandArgs);
                break;

            case "config":
                new Config().Run(commandArgs);
                break;

            case "destroy":
            case "terminate":
                new Destroy().Run(commandArgs);
                break;

            case "migrate":
                new Migrate().Run(commandArgs);
                break;

            case "export":
                new Export().Run(commandArgs);
                break;

            case "gpg-start-agent":
                new Gpg().Run(new List<string> { "start" });
                break;

            case "gpg-stop-agent":
                new Gpg().Run(new List<string> { "stop" });
                break;

            case "gpg-restart-agent":
                new Gpg().Run(new List<string> { "restart" });
                break;

            default:
                AnsiConsole.MarkupLine("[red]Invalid command[/]");
                break;
        }

        AppService.Instance.ReleaseLock();
    }

    public static string GetErrorColor(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Error => "red",
            ErrorSeverity.Info => "blue",
            ErrorSeverity.Success => "green",
            ErrorSeverity.Warning => "yellow",
            _ => "white",
        };
    }

    public static bool AcquireLock()
    {
        if (AppService.Instance.AcquireLock()) return true;

        AnsiConsole.MarkupLine(
            "[red]Unable to acquire lock. There must be another instance of Horus currently running[/]");
        return false;
    }

    #endregion

    #region Private methods

    private void InteractiveMode()
    {
        Console.Clear();

        while (true)
        {
            var input = AnsiConsole.Ask<string>("[blue]Horus:[/]");
            var args = input.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (args.FirstOrDefault() is "quit" or "exit" or "q") break;

            Run(args);
            AnsiConsole.Write(new Rule());
        }

        AnsiConsole.MarkupLine("[green]bye[/]");
    }

    private static List<string> HandleAliases(List<string> args)
    {
        if (args.Count == 0)
        {
            args.Add("interactive");
            return args;
        }

        if (args[0] == "get")
        {
            return new List<string> { "show", "-c", args[1] };
        }

        if (args[0] == "show" && args.Contains("-a"))
        {
            return new List<string> { "show", "-f", "-p", "-m", args.Last() };
        }

        return args;
    }

    private static void SetAppLanguage()
    {
        var (settings, error) = AppService.Instance.GetSettings();
        if (error is null && settings is not null)
        {
            Locale.SetLanguage(settings.Language);
        }
    }

    #endregion
}