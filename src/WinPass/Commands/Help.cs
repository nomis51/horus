using System.Diagnostics;
using WinPass.Commands.Abstractions;

namespace WinPass.Commands;

public class Help : ICommand
{
    #region Constants

    private const string CommandsPageUrl = "https://github.com/nomis51/winpass/docs/commands.md";

    #endregion

    #region Public methods

    public void Run(List<string> args)
    {
        Process.Start(CommandsPageUrl);
    }

    #endregion
}