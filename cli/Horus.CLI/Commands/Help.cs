using System.Diagnostics;
using System.Runtime.InteropServices;
using Horus.Commands.Abstractions;

namespace Horus.Commands;

public class Help : ICommand
{
    #region Constants

    private const string CommandsPageUrl = "https://github.com/nomis51/winpass/docs/commands.md";

    #endregion

    #region Public methods

    public void Run(List<string> args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", CommandsPageUrl);
        }
        else
        {
            Process.Start(new ProcessStartInfo(CommandsPageUrl)
            {
                UseShellExecute = true
            });
        }
    }

    #endregion
}