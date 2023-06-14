using System.Diagnostics;
using System.Reflection;

namespace WinPass.Shared.Helpers;

public static class ProcessHelper
{
    #region Public methods

    public static void Fork(string[] args, string workingDirectory = "")
    {
        var asm = Assembly.GetEntryAssembly();
        if (asm is null) return;
        
        _ = Exec(asm.Location.Replace(".dll", ".exe"), args, workingDirectory, false);
    }

    public static Tuple<bool, string, string> Exec(string program, string[] args, string workingDirectory = "",
        bool waitForExit = true)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = program,
                WorkingDirectory = workingDirectory,
                Arguments = string.Join(" ", args),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = waitForExit,
                RedirectStandardOutput = waitForExit,
            },
        };

        process.Start();
        if (!waitForExit) return new Tuple<bool, string, string>(false, string.Empty, string.Empty);

        var stderr = process.StandardError.ReadToEnd();
        var stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return Tuple.Create(process.ExitCode == 0, stdout, stderr);
    }

    #endregion
}