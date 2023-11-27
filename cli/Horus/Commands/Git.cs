using Horus.Commands.Abstractions;
using Horus.Core.Services;
using Horus.Shared.Models.Errors.Fs;
using Spectre.Console;

namespace Horus.Commands;

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