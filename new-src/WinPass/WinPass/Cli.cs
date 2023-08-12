using Spectre.Console;
using WinPass.Commands;
using WinPass.Core.Services;
using WinPass.Shared.Enums;

namespace WinPass;

public class Cli
{
    #region Public methods

    public void Run(string[] args)
    {
        var (_, error) = AppService.Instance.Verify();
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[red]{error.Message}[/]");
            return;
        }

        if (args.Length == 0)
        {
            args = new[] { "ls" };
        }

        var commandArgs = args.Skip(1).ToList();
        switch (args[0])
        {
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
                new Destroy().Run(commandArgs);
                break;

            case "migrate":
                new Migrate().Run(commandArgs);
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
            "[red]Unable to acquire lock. There must be another instance of winpass currently running[/]");
        return false;
    }

    #endregion
}