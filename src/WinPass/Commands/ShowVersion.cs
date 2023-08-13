using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Shared;
using WinPass.Shared.Helpers;

namespace WinPass.Commands;

public class ShowVersion : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        var version = VersionHelper.GetVersion();
        AnsiConsole.MarkupLine($"{Locale.Get("version")} {version.Major}.{version.Minor}.{version.Build}");
    }

    #endregion
}