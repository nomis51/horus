using Horus.Commands.Abstractions;
using Horus.Shared;
using Horus.Shared.Helpers;
using Spectre.Console;

namespace Horus.Commands;

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