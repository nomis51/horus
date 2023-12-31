﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Horus.Helpers;

public class TerminalHelper
{
    #region Constants

    private const string WindowsTerminalName = "wt.exe";
    private const string PowerShell7Name = "pwsh.exe";
    private const string PowerShellName = "powershell.exe";
    private const string CmdName = "cmd.exe";
    private const string ShName = "sh";

    #endregion

    #region Public methods

    public static void SpawnTerminal(string workingDirectory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Spawn(WindowsTerminalName, workingDirectory)) return;
            if (Spawn(PowerShell7Name, workingDirectory)) return;
            if (Spawn(PowerShellName, workingDirectory)) return;

            Spawn(CmdName, workingDirectory);
        }
        else
        {
            Spawn(ShName, workingDirectory);
        }
    }

    #endregion

    #region Private methods

    private static bool Spawn(string exe, string wd)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = exe,
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? string.Join(" ", GetExeArguments(exe, wd))
                    : string.Join(" ", $"-c 'cd {wd}; exec \"" + "${SHELL:-sh}" + "\"'")
            });
            return process is not null && (!process.HasExited || process.ExitCode == 0);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string[] GetExeArguments(string exe, string wd)
    {
        return exe switch
        {
            WindowsTerminalName => new[] { "-d", wd },
            PowerShell7Name => new[] { "-workingdirectory", wd },
            PowerShellName => new[] { "-NoExit", "-Command", $"\"Set-Location {wd}\"" },
            _ => new[] { "/K", $"\"cd {wd}\"" }
        };
    }

    #endregion
}