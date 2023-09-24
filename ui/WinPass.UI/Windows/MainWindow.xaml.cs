using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WinPass.Core;
using WinPass.Core.Services;
using WinPass.UI.Extensions;

namespace WinPass.UI.Windows;

public partial class MainWindow
{
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
        await WebView.WebView.EnsureCoreWebView2Async();
        WebView.WebView.CoreWebView2.Settings.IsZoomControlEnabled = false;

#if DEBUG
        WebView.WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        WebView.WebView.CoreWebView2.OpenDevToolsWindow();
#else
        WebView.WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
#endif
    }

    #endregion
}