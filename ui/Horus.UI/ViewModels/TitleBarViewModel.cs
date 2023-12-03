using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Horus.Core.Services;
using Horus.UI.Helpers;
using ReactiveUI;
using Serilog;

namespace Horus.UI.ViewModels;

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