using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Horus.UI.Enums;
using Horus.UI.Services;
using Horus.UI.ViewModels;

namespace Horus.UI.Windows;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    #region Members

    private bool _isDraggingWindow;
    private Point _dragStartPoint;

    #endregion

    #region Constructors

    public MainWindow()
    {
        InitializeComponent();

        DialogService.Instance.OnShow += DialogService_OnShow;
    }

    #endregion

    #region Private methods

    private void DialogService_OnShow(DialogType dialogType, object? data)
    {
        DialogManagerView.ShowDialog(dialogType, data);
    }

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

    private void TitleBarView_OnOpenSettings()
    {
        DialogService.Instance.Show(DialogType.Settings);
    }

    private void EntryFormView_OnEntryRenamedEntry()
    {
        EntryListView.ReloadList();
    }

    private void DialogManagerView_OnClose(DialogType dialogType, object? data)
    {
        switch (dialogType)
        {
            case DialogType.DuplicateEntry:
                if (data is true) EntryListView.ReloadList();
                break;

            case DialogType.NewEntry:
                if (data is not string name) return;
                if (string.IsNullOrWhiteSpace(name)) return;

                EntryListView.ReloadList();
                ViewModel!.EntrySelected = true;
                EntryFormView.SetEntryItem(name);
                break;
        }
    }

    #endregion
}