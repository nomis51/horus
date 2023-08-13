using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;

namespace WinPass.Commands;

public class Destroy : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        if (!Cli.AcquireLock()) return;

        var (isAhead, isAheadError) = AppService.Instance.GitIsAheadOfRemote();
        if (isAheadError is not null)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(isAheadError.Severity)}]{isAheadError.Message}[/]");
            return;
        }

        var choiceSync = Locale.Get("y");
        if (isAhead)
        {
            AnsiConsole.MarkupLine($"[yellow]{Locale.Get("repositoryAhead")}[/]");
            choiceSync =
                AnsiConsole.Ask(Locale.Get("questions.syncChangesBeforeDelete"), Locale.Get("y"));
            if (choiceSync == Locale.Get("y"))
            {
                var resultGitPush = AppService.Instance.GitPush();
                if (resultGitPush.HasError)
                {
                    AnsiConsole.MarkupLine(
                        $"[{Cli.GetErrorColor(resultGitPush.Error!.Severity)}]{resultGitPush.Error!.Message}[/]");
                    return;
                }
            }
        }

        var choiceConfirmDelete =
            AnsiConsole.Ask($"{Locale.Get("questions.confirmDestroyStore")}?",
                Locale.Get("n"));
        if (choiceConfirmDelete != Locale.Get("y")) return;

        if (isAhead && choiceSync != Locale.Get("y"))
        {
            var (repositoryName, repositoryNameError) = AppService.Instance.GitGetRemoteRepositoryName();
            if (repositoryNameError is not null)
            {
                AnsiConsole.MarkupLine(
                    $"[{Cli.GetErrorColor(repositoryNameError.Severity)}]{repositoryNameError.Message}[/]");
                return;
            }

            AnsiConsole.MarkupLine(
                "Are you [yellow]really[/] sure you want to [red]delete[/] the store and [yellow]NOT[/] push local changes to the remote repository?");
            _ = AnsiConsole.Prompt(
                new TextPrompt<string>(
                        $"If yes, please confirm by typing the name of the remote repository [green]({repositoryName})[/]: ")
                    .Validate(v => v == repositoryName)
                    .ValidationErrorMessage("Repository name doesn't match")
            );
        }

        var resultDestroyStore = AppService.Instance.DestroyStore();
        if (resultDestroyStore.HasError)
        {
            AnsiConsole.MarkupLine(
                $"[{Cli.GetErrorColor(resultDestroyStore.Error!.Severity)}]{resultDestroyStore.Error!.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]{Locale.Get("storeDestroyed")}[/]");
    }

    #endregion
}