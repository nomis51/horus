using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Models.Display;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Search : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        if (!Cli.AcquireLock()) return;
        if (!AppService.Instance.IsStoreInitialized())
        {
            AnsiConsole.MarkupLine($"[red]{new FsStoreNotInitializedError().Message}[/]");
            return;
        }

        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("cli.args.searchTermRequired")}[/]");
            return;
        }

        var searchMetadatas = args.Contains("-m");
        var text = args.Last();

        if (searchMetadatas && !AppService.Instance.VerifyLock())
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.getLockFailed")}[/]");
            return;
        }

        List<StoreEntry> entries = new();

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start(Locale.Get("searching"), _ =>
            {
                var (results, error) = AppService.Instance.SearchStoreEntries(text, searchMetadatas);
                if (error is not null)
                {
                    AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(error.Severity)}]{error.Message}[/]");
                    return;
                }

                entries = results;
            });

        if (!entries.Any())
        {
            AnsiConsole.MarkupLine(Locale.Get("notEntryFound"));
            return;
        }

        var tree = new Tree(string.Empty);
        List.RenderEntries(entries, tree);
        AnsiConsole.Write(tree);
    }

    #endregion
}