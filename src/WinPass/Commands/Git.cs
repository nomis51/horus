using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Git : ICommand
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

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Running git command...", _ =>
            {
                var result = AppService.Instance.ExecuteGitCommand(args.ToArray());
                AnsiConsole.WriteLine(result);
            });
    }

    #endregion
}