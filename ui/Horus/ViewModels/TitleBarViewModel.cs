using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Horus.Core.Services;
using Horus.Enums;
using Horus.Helpers;
using Horus.Services;
using ReactiveUI;
using Serilog;

namespace Horus.ViewModels;

public class TitleBarViewModel : ViewModelBase
{
    #region Props

    public SyncButtonViewModel SyncButtonViewModel { get; set; } = new();

    public string Title => nameof(Horus);
    public int TitleSize => 18;
    public int LogoSize => 20;
    public int ButtonsSize => 30;
    public int SystemButtonsWidth => 38;

    private bool _isUpdateIconVisible;

    public bool IsUpdateIconVisible
    {
        get => _isUpdateIconVisible;
        set
        {
            this.RaiseAndSetIfChanged(ref _isUpdateIconVisible, value);
            this.RaisePropertyChanged(nameof(UpdateMessage));
        }
    }

    public string UpdateMessage { get; private set; } = string.Empty;

    #endregion

    #region Public methods

    public void RestartGpg()
    {
        var (result, error) = AppService.Instance.RestartGpgAgent();
        if (error is not null)
        {
            Log.Warning("Failed to restart GPG agent: {Message}. {Output}", error.Message, result);
            SnackbarService.Instance.Show("Failed to restart GPG agent", SnackbarSeverity.Warning, 5000);
        }
        else
        {
            Log.Information("GPG agent restarted: {Message}", result);
            SnackbarService.Instance.Show("GPG agent restarted", SnackbarSeverity.Success);
        }
    }

    public void StopGpg()
    {
        var (result, error) = AppService.Instance.StopGpgAgent();
        if (error is not null)
        {
            SnackbarService.Instance.Show("Failed to stop GPG agent", SnackbarSeverity.Warning, 5000);
            Log.Warning("Failed to stop GPG agent: {Message}. {Output}", error.Message, result);
        }
        else
        {
            Log.Information("GPG agent stopped: {Message}", result);
            SnackbarService.Instance.Show("GPG agent stopped", SnackbarSeverity.Success);
        }
    }
    
    public void StartGpg()
    {
        var (result, error) = AppService.Instance.StopGpgAgent();
        if (error is not null)
        {
            SnackbarService.Instance.Show("Failed to start GPG agent", SnackbarSeverity.Warning, 5000);
            Log.Warning("Failed to start GPG agent: {Message}. {Output}", error.Message, result);
        }
        else
        {
            Log.Information("GPG agent started: {Message}", result);
            SnackbarService.Instance.Show("GPG agent started", SnackbarSeverity.Success);
        }
    }

    public void ShowUpdateIcon(string version)
    {
        UpdateMessage = $"New version {version} ready to be installed after a restart";
        IsUpdateIconVisible = true;
    }

    public void OpenGitHubPage()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", App.GitHubPageUrl);
            }
            else
            {
                Process.Start(App.GitHubPageUrl);
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to open '{Url}': {Message}", App.GitHubPageUrl, e.Message);
        }
    }

    public void OpenTerminal()
    {
        TerminalHelper.SpawnTerminal(AppService.Instance.GetStoreLocation());
    }

    #endregion
}