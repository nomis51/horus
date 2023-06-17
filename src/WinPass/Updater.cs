using System.Reflection;
using Spectre.Console;
using Squirrel;

namespace WinPass;

public static class Updater
{
    #region Public methods

    public static async Task Verify()
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
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Unable to update: {e.Message}[/]");
        }
    }

    #endregion
}