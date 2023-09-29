using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using WinPass.Core;
using WinPass.Core.Services;
using WinPass.UI.Extensions;
using WinPass.UI.Helpers;

namespace WinPass.UI.Windows;

public partial class MainWindow
{
    #region Constants

    private static readonly string GithubPage = "https://github.com/nomis51/winpass";

    #endregion

    #region Constructors

    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();

        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new FsService(".winpass-tests"),
            new GitService(),
            new GpgService(),
            new SettingsService()
        ));
    }

    #endregion

    #region Private methods

    private void InitializeServices()
    {
        var services = new ServiceCollection();
        services.AddServices();
        Resources.Add("services", services.BuildServiceProvider());
    }

    private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        TaskbarIcon.Icon = new BitmapImage(new Uri("pack://application:,,,/WinPass;component/Resources/Assets/winpass-logo.ico", UriKind.Absolute).ToString());

        await WebView.WebView.EnsureCoreWebView2Async();
        ButtonSettings.IsEnabled = true;
        WebView.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;

#if DEBUG
        WebView.WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        WebView.WebView.CoreWebView2.OpenDevToolsWindow();
#else
        WebView.WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif
    }

    private void ButtonGitHub_OnClick(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer.exe", GithubPage);
    }

    private void ButtonTerminal_OnClick(object sender, RoutedEventArgs e)
    {
        TerminalHelper.SpawnTerminal(AppService.Instance.GetStoreLocation());
    }

    private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
    {
        WebView.WebView.CoreWebView2.ExecuteScriptAsync("window.winpass.openSettings()");
    }

    private void MenuItemQuit_OnClick(object sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    #endregion
}