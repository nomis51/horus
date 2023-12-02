using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

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

    #endregion

    #region Constructors

    public TitleBar()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

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

    #endregion
}