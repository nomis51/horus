using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Horus.Enums;
using Horus.Extensions;
using Horus.Services;
using Horus.ViewModels;

namespace Horus.Views;

public partial class TitleBar : ViewBase<TitleBarViewModel>
{
    #region Events

    public delegate void WindowTitleBarPressedEvent(int x, int y);

    public event WindowTitleBarPressedEvent? WindowTitleBarPressed;

    public delegate void WindowTitleBarReleasedEvent();

    public event WindowTitleBarReleasedEvent? WindowTitleBarReleased;

    public delegate void WindowTitleBarMoveEvent(int x, int y);

    public event WindowTitleBarMoveEvent? WindowTitleBarMove;

    public delegate void WindowCloseEvent();

    public event WindowCloseEvent? WindowClose;

    public delegate void WindowMinimizeEvent();

    public event WindowMinimizeEvent? WindowMinimize;

    public delegate void WindowMaximizeEvent();

    public event WindowMaximizeEvent? WindowMaximize;

    public delegate void OpenSettingsEvent();

    public event OpenSettingsEvent? OpenSettings;

    public delegate void ActiveStoreChangedEvent();

    public event ActiveStoreChangedEvent? ActiveStoreChanged;

    #endregion

    #region Constructors

    public TitleBar()
    {
        InitializeComponent();

        DialogService.Instance.OnClose += DialogService_OnClose;
    }

    #endregion

    #region Public methods

    public void UpdateAvailable(string version)
    {
        ViewModel?.ShowUpdateIcon(version);
    }

    #endregion

    #region Private methods

    private void DialogService_OnClose(DialogType dialogType, object? data)
    {
        if (dialogType != DialogType.CreateStore || data is not true) return;

        Dispatch(vm =>
        {
            vm?.RetrieveActiveStore();
            vm?.RetrieveStores();
        });
    }

    private void TitleBar_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        var position = e.GetPosition(desktop.MainWindow);
        WindowTitleBarMove?.Invoke((int)position.X, (int)position.Y);
    }

    private void TitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

        var position = e.GetPosition(desktop.MainWindow);
        WindowTitleBarPressed?.Invoke((int)position.X, (int)position.Y);
    }

    private void TitleBar_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        WindowTitleBarReleased?.Invoke();
    }

    private void Ignore_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }


    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowClose?.Invoke();
    }

    private void ButtonMinimize_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowMinimize?.Invoke();
    }

    private void ButtonMaximize_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowMaximize?.Invoke();
    }


    private void ButtonOpenGitHubPage_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.OpenGitHubPage();
    }

    private void ButtonOpenTerminal_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.OpenTerminal();
    }

    private void ButtonOpenSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenSettings?.Invoke();
    }

    private void MenuItemRestartGpg_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.RestartGpg());
    }

    private void MenuItemStopGpg_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.StopGpg());
    }

    private void MenuItemStartGpg_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.StartGpg());
    }

    private void ButtonStore_OnClick(object? sender, RoutedEventArgs e)
    {
        var name = sender!.GetTag<string>();
        if (name.StartsWith('$')) return;

        Dispatch(vm =>
        {
            if (!vm!.ChangeStore(name)) return;

            InvokeUi(() => ActiveStoreChanged?.Invoke());
        });
    }

    private void ButtonCreateStore_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogService.Instance.Show(DialogType.CreateStore);
    }

    private void ButtonExportStore_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;
        
        Dispatch(vm=>vm?.ExportStore(topLevel));
    }

    #endregion
}