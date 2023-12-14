using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Horus.Enums;
using Horus.Services;
using Horus.ViewModels;

namespace Horus.Windows;

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
        SnackbarService.Instance.OnShow += SnackbarManager_OnShow;
        SpinnerOverlayService.Instance.OnShow += SpinnerOverlay_OnShow;
        SpinnerOverlayService.Instance.OnHide += SpinnerOverlay_OnHide;

        Loaded += OnLoaded;
    }

    #endregion

    #region Public methods

    public void UpdateAvailable(string version)
    {
        TitleBar.UpdateAvailable(version);
    }

    #endregion

    #region Private methods

    private void SpinnerOverlay_OnHide()
    {
        Dispatch(vm => { vm!.IsLoading = false; });
    }

    private void SpinnerOverlay_OnShow(string message)
    {
        Dispatch(vm =>
        {
            vm!.LoadingMessage = message;
            vm.IsLoading = true;
        });
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Height += 1;
        VerifyStoreInitialized();
    }

    private void VerifyStoreInitialized()
    {
        if (ViewModel!.IsStoreInitialized())
        {
            ViewModel!.IsLoading = false;
            return;
        }

        DialogService.Instance.Show(DialogType.InitializeStore);
    }

    private void SnackbarManager_OnShow(string message, SnackbarSeverity severity, int duration)
    {
        SnackbarManager.ShowSnackbar(message, severity, duration);
    }

    private void DialogService_OnShow(DialogType dialogType, object? data)
    {
        DialogManager.ShowDialog(dialogType, data);
    }

    private void EntryList_OnEntrySelected(string name)
    {
        ViewModel!.EntrySelected = !string.IsNullOrWhiteSpace(name);
        EntryForm.SetEntryItem(name);
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

    private void EntryForm_OnEntryRenamedEntry()
    {
        EntryList.ReloadList();
    }

    private void DialogManager_OnClose(DialogType dialogType, object? data)
    {
        switch (dialogType)
        {
            case DialogType.DuplicateEntry:
                if (data is true) EntryList.ReloadList();
                break;

            case DialogType.DeleteEntry:
                if (data is true)
                {
                    EntryList.ReloadList(true);
                    ViewModel!.EntrySelected = false;
                }

                break;

            case DialogType.NewEntry:
                if (data is not string name) return;
                if (string.IsNullOrWhiteSpace(name)) return;

                EntryList.ReloadList();
                ViewModel!.EntrySelected = true;
                EntryForm.SetEntryItem(name);
                break;

            case DialogType.InitializeStore:
                EntryList.ReloadList(true);
                ViewModel!.IsLoading = false;
                break;

            case DialogType.DestroyStore:
                VerifyStoreInitialized();
                break;

            case DialogType.CreateStore:
                EntryList.ReloadList();
                ViewModel!.EntrySelected = false;
                EntryForm.SetEntryItem(string.Empty);
                EntryList.TreeView.SelectedItem = null;
                break;
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (e.CloseReason == WindowCloseReason.WindowClosing)
        {
            e.Cancel = true;
            ShowInTaskbar = false;
            Hide();
        }
    }

    private void WindowBase_OnResized(object? sender, WindowResizedEventArgs e)
    {
        EntryList.WindowResized(Height);
        EntryForm.WindowResized(Height);
    }


    private void EntryForm_OnEntryClosed()
    {
        ViewModel!.EntrySelected = false;
        EntryForm.SetEntryItem(string.Empty);
        EntryList.TreeView.SelectedItem = null;
    }


    private void TitleBar_OnActiveStoreChanged()
    {
        EntryList.ReloadList();
        ViewModel!.EntrySelected = false;
        EntryForm.SetEntryItem(string.Empty);
        EntryList.TreeView.SelectedItem = null;
    }

    #endregion
}