﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Squirrel;

namespace Horus.Helpers;

public static class UpdateHelper
{
    #region Public methods

    public static async Task<string> CheckForUpdates()
    {
        try
        {
            Log.Information("Checking for updates");
            using var updateManager = new UpdateManager(App.GitHubPageUrl);
            var info = await updateManager.CheckForUpdate();

            if (info.ReleasesToApply.Count == 0) return string.Empty;

            var version = info.ReleasesToApply.Last().Version.ToString();
            Log.Information("Downloading update {Version}", version);

            await updateManager.UpdateApp();
            Log.Information("Update {Version} downloaded", version);
            
            return version;
        }
        catch (Exception e)
        {
            Log.Warning("Failed to check / download updates: {Message}", e.Message);
        }

        return string.Empty;
    }

    #endregion
}