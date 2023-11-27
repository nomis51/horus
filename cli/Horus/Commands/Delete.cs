using Horus.Commands.Abstractions;
using Horus.Core.Services;
using Horus.Shared;
using Horus.Shared.Models.Errors.Fs;
using Spectre.Console;

namespace Horus.Commands;

public class Delete : ICommand
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
            AnsiConsole.MarkupLine($"[red]{Locale.Get("cli.args.passwordNameRequired")}[/]");
            return;
        }

        var name = args.Last();

        if (!AppService.Instance.DoStoreEntryExists(name))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.passwordDoesntExists")}[/]");
            return; 
        }

        var choice = AnsiConsole
            .Ask<string>(Locale.Get("questions.confirmDeletePassword", new object[] { name }), "n")
            .ToLower();

        if (choice != Locale.Get("y")) return;

        var result = AppService.Instance.DeleteStoreEntry(name);

        if (result.HasError)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(result.Error!.Severity)}]{result.Error!.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]{Locale.Get("passwordRemoved", new object[] { name })}[/]");
    }

    #endregion
}