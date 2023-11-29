using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Horus.UI.ViewModels;

namespace Horus.UI.Windows;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    #region Members

    private bool _isDraggingWindow = false;
    private Point _dragStartPoint;

    #endregion

    #region Constructors

    public MainWindow()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void EntryListView_OnEntrySelected(string name)
    {
        ViewModel!.EntrySelected = !string.IsNullOrWhiteSpace(name);
        EntryFormView.SetEntryItem(name);
    }

    private void ButtonCloseSnackbar_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CloseSnackbar();
    }

    private void TitleBar_OnWindowTitleBarPressed(int x, int y)
    {
        if (WindowState is WindowState.FullScreen or WindowState.Minimized) return;

        _isDraggingWindow = true;
        _dragStartPoint = new Point(x, y);
    }

    private void TitleBar_OnWindowTitleBarReleased()
    {
        _isDraggingWindow = false;
    }

    private void TitleBar_OnWindowTitleBarMove(int x, int y)
    {
        if (!_isDraggingWindow) return;

        Position = new PixelPoint(
            Position.X + (int)(x - _dragStartPoint.X),
            Position.Y + (int)(y - _dragStartPoint.Y)
        );
    }

    private void TitleBar_OnWindowClose()
    {
        Close();
    }

    private void TitleBar_OnWindowMaximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void TitleBar_OnWindowMinimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void NewEntryDialogView_OnClose(string name)
    {
        ViewModel?.CloseNewEntryDialog();
        if (string.IsNullOrWhiteSpace(name)) return;
        if (!ViewModel!.CreateEntry(name)) return;

        EntryListView.ReloadList();
        ViewModel!.EntrySelected = true;
        EntryFormView.SetEntryItem(name);
    }

    private void EntryListView_OnCreateEntry()
    {
        ViewModel?.OpenNewEntryDialog();
    }

    private void SettingsView_OnClose()
    {
        ViewModel?.CloseSettingsDialog();
    }

    private void TitleBarView_OnOpenSettings()
    {
        ViewModel?.OpenSettingsDialog();
    }

    #endregion
}