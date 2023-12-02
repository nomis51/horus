using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Horus.Core.Services;
using Horus.UI.Helpers;
using Horus.UI.ViewModels;
using MainWindow = Horus.UI.Windows.MainWindow;

namespace Horus.UI;

public partial class App : Application
{
    #region Constants

    public const string GitHubPageUrl = "https://github.com/nomis51/horus";
    public const string GitHubIssuePageUrl = $"{GitHubPageUrl}/issues";

    #endregion

    #region Public methods

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    #endregion

    #region Private methods

    private void MenuItemOpenGitHub_OnClick(object? sender, EventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", GitHubPageUrl);
        }
    }

    private void MenuItemOpenGitHubIssue_OnClick(object? sender, EventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", GitHubIssuePageUrl);
        }
    }

    private void MenuItemOpenTerminal_OnClick(object? sender, EventArgs e)
    {
        TerminalHelper.SpawnTerminal(AppService.Instance.GetStoreLocation());
    }

    private void MenuItemOpenLogs_OnClick(object? sender, EventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", AppService.Instance.GetLogsLocation().Replace("/", "\\"));
        }
    }

    private void MenuItemQuit_OnClick(object? sender, EventArgs e)
    {
        Environment.Exit(0);
    }

    #endregion
}