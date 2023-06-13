using System.Diagnostics;

namespace WinPass.Shared.Helpers;

public static class ProcessHelper
{
    #region Public methods

    public static Tuple<bool, string, string> Exec(string program, string[] args, string workingDirectory = ".")
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            },
        };

        process.Start();
        var stderr = process.StandardError.ReadToEnd();
        var stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return Tuple.Create(process.ExitCode == 0, stdout, stderr);
    }

    #endregion
}