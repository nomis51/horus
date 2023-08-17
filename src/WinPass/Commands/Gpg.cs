using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Gpg : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        Result<string, Error?> result;

        switch (args[0])
        {
            case "start":
                result = AppService.Instance.StartGpgAgent();
                break;

            case "stop":
                result = AppService.Instance.StopGpgAgent();
                break;

            case "restart":
                result = AppService.Instance.RestartGpgAgent();
                break;

            default:
                return;
        }

        if (result.Item2 is not null)
        {
            AnsiConsole.MarkupLine($"[${Cli.GetErrorColor(result.Item2.Severity)}]{result.Item2.Message}[/]");
            return;
        }

        AnsiConsole.WriteLine(result.Item1);
    }

    #endregion
}