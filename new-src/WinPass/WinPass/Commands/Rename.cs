using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Rename : ICommand
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

        var name = args[^1];
        var duplicate = false;

        for (var i = 0; i < args.Count - 1; ++i)
        {
            if (args[i] == "-d")
            {
                duplicate = true;
            }
        }

        AnsiConsole.Write($"{Locale.Get("questions.enterNewName")}: ");
        var newName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(newName))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.nameIsEmpty")}[/]");
            return;
        }

        var choice =
            AnsiConsole.Ask(
                Locale.Get("questions.confirmWantsToRenamePassword", new object[]
                {
                    duplicate ? Locale.Get("duplicate") : Locale.Get("rename"),
                    name,
                    newName
                }),
                Locale.Get("n")).ToLower();

        if (choice != Locale.Get("y")) return;

        var result = AppService.Instance.RenameStoreEntry(name, newName, duplicate);

        if (result.HasError)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(result.Error!.Severity)}]{result.Error!.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine(duplicate
            ? $"[green]{Locale.Get("passwordDuplicated", new object[] { name, newName })}[/]"
            : $"[green]{Locale.Get("passwordRenamed", new object[] { name, newName })}[/]");

    }

    #endregion
}