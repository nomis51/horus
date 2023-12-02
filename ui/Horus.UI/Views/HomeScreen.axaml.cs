using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Horus.Core.Services;
using Horus.UI.Enums;
using Horus.UI.Helpers;
using Horus.UI.Services;

namespace Horus.UI.Views;

public partial class HomeScreen : UserControl
{
    #region Constructors

    public HomeScreen()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ButtonCreateEntry_OnClick(object? sender, RoutedEventArgs e)
    {
       DialogService.Instance.Show(DialogType.NewEntry);
    }

    private void ButtonOpenSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogService.Instance.Show(DialogType.Settings);
    }

    private void ButtonOpenTerminal_OnClick(object? sender, RoutedEventArgs e)
    {
        TerminalHelper.SpawnTerminal(AppService.Instance.GetStoreLocation());
    }

    private void ButtonOpenGitHub_OnClick(object? sender, RoutedEventArgs e)
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

    #endregion
}