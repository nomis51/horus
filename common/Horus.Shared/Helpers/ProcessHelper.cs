using Horus.Shared.Models.Terminal;
using Serilog;

namespace Horus.Shared.Helpers;

public static class ProcessHelper
{
    #region Public methods

    public static void Fork(IEnumerable<string> args, string workingDirectory = "")
    {
        new TerminalSession(workingDirectory, false)
            .Command(new[]
            {
                nameof(Horus) + ".exe",
            }.Concat(args))
            .Execute();
    }

    #endregion
}