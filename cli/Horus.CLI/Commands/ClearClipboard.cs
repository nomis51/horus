using Horus.Commands.Abstractions;
using Horus.Shared.Extensions;
using Horus.Shared.Helpers;

namespace Horus.Commands;

public class ClearClipboard : ICommand
{
    #region Constants

    private const int NbClipboardPollutionIterations = 20;

    #endregion
    
    #region Public methods

    public void Run(List<string> args)
    {
        if (args.Count == 0) return;
        if (!int.TryParse(args[0], out var intValue)) return;

        Thread.Sleep(1000 * intValue);

        // To polluate any clipboard history tracker
        for (var i = 0; i < NbClipboardPollutionIterations; ++i)
        {
            ClipboardHelper.Copy(Guid.NewGuid().ToString().ToBase64());
        }

        ClipboardHelper.Clear();
    }

    #endregion
}