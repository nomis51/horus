using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Horus.Shared.Models.Terminal;

public class TerminalSession
{
    #region Members

    private readonly Process _process = new()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        },
    };

    private readonly List<List<string>> _arguments = new();
    private readonly bool _waitForExit;

    #endregion

    #region Constructors

    public TerminalSession(string workingDirectory = ".", bool waitForExit = true)
    {
        _process.StartInfo.WorkingDirectory = workingDirectory;
        _process.StartInfo.RedirectStandardError = waitForExit;
        _process.StartInfo.RedirectStandardOutput = waitForExit;
        _waitForExit = waitForExit;
    }

    #endregion

    #region Public methods

    public TerminalSession Command(IEnumerable<string> arguments)
    {
        _arguments.Add(arguments.ToList());
        return this;
    }

    public TerminalSessionResult Execute()
    {
        var pipedCommands = string.Join(" | ", _arguments.Select(args => string.Join(" ", args)));
        _process.StartInfo.Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"/C {pipedCommands}"
            : $"-c '{pipedCommands}'";
        _process.Start();
        if (!_waitForExit) return new TerminalSessionResult();

        _process.WaitForExit();
        var stderr = _process.StandardError.ReadToEnd();
        var stdout = _process.StandardOutput.ReadToEnd();

        return new TerminalSessionResult(
            _process.ExitCode == 0,
            stderr.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
            stdout.Split(Environment.NewLine, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        );
    }

    #endregion
}