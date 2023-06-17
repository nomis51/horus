﻿using System.Reflection;
using Spectre.Console;
using Squirrel;

namespace WinPass;

public static class Updater
{
    #region Public methods

    public static async Task Verify()
    {
        EnsureAppLinked();
        await CheckForUpdate();
    }

    public static async Task Update()
    {
        try
        {
            using var updateManager = await UpdateManager.GitHubUpdateManager("https://github.com/nomis51/WinPass");
            var info = await updateManager.CheckForUpdate();

            if (!info.ReleasesToApply.Any())
            {
                AnsiConsole.MarkupLine("No update available");
                return;
            }

            AnsiConsole.MarkupLine("Updating...");
            await updateManager.UpdateApp();
            EnsureAppLinked();
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Unable to update: {e.Message}[/]");
        }
    }

    #endregion

    #region Private methods

    private static void EnsureAppLinked()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        var filePath = assembly.Location.Replace(".dll", ".exe");

        AddToPath(Path.GetDirectoryName(filePath)!);
    }

    private static void AddToPath(string path)
    {
        var name = "PATH";
        var scope = EnvironmentVariableTarget.User;
        var oldValue = Environment.GetEnvironmentVariable(name, scope);
        if (string.IsNullOrEmpty(oldValue))
        {
            oldValue = string.Empty;
        }

        if (oldValue.Contains(path)) return;

        var newValue = oldValue + $";{path}";
        Environment.SetEnvironmentVariable(name, newValue, scope);
    }

    private static async Task CheckForUpdate()
    {
        try
        {
            using var updateManager = await UpdateManager.GitHubUpdateManager("https://github.com/nomis51/WinPass");
            var info = await updateManager.CheckForUpdate();

            if (!info.ReleasesToApply.Any()) return;

            AnsiConsole.MarkupLine(
                "[yellow]New version available! Run 'winpass update' to update to the latest version.[/]");
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[yellow]Unable to check for updates: {e.Message}[/]");
        }
    }

    #endregion
}