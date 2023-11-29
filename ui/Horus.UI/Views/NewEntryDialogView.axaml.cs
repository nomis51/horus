using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public partial class NewEntryDialogView : ViewBase<NewEntryDialogViewModel>
{
    #region Events

    public delegate void CloseEvent(string name = "");

    public event CloseEvent? Close;

    #endregion

    #region Constructors

    public NewEntryDialogView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    #endregion

    #region Private methods

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { TextBoxName.Focus(); });
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.CreateEntry()) return;

            InvokeUi(() => Close?.Invoke(vm.Name));
        });
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    #endregion
}