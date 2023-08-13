using System.Text.RegularExpressions;
using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;

namespace WinPass.Commands;

public class Init : ICommand
{
    #region Constants

    private static readonly Regex RegGpgKeyId =
        new("[a-z0-9]{40}|[a-z0-9]{16}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion

    #region Public methods

    public void Run(List<string> args)
    {
        var gpgId = AnsiConsole.Ask<string>($"{Locale.Get("questions.gpgId")}: ");

        var match = RegGpgKeyId.Match(gpgId);
        if (!match.Success || match.Index != 0 || match.Length != gpgId.Length)
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.invalidGpgKey")}[/]");
            return;
        }

        var gitUrl = AnsiConsole.Ask<string>($"{Locale.Get("questions.gitRepositoryUrl")}: ");
        if (string.IsNullOrEmpty(gitUrl))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.gitUrlEmpty")}[/]");
            return;
        }

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Initializing...", _ =>
            {
                var result = AppService.Instance.InitializeStoreFolder(gpgId, gitUrl);
                if (result.HasError)
                {
                    AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(result.Error!.Severity)}]{result.Error!.Message}[/]");
                    return;
                }

                AnsiConsole.MarkupLine($"[green]{Locale.Get("storeInitialized")}[/]");
            });
    }

    #endregion
}