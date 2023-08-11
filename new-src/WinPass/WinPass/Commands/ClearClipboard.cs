using WinPass.Commands.Abstractions;
using WinPass.Shared.Helpers;

namespace WinPass.Commands;

public class ClearClipboard : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        if (args.Count == 0) return;
        if (!int.TryParse(args[0], out var intValue)) return;

        Thread.Sleep(1000 * intValue);
        ClipboardHelper.Clear();
    }

    #endregion
}