using Avalonia.Interactivity;
using Avalonia.Threading;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public partial class DeleteEntryDialogView : ViewBase<DeleteEntryDialogViewModel>
{
    #region Events

    public delegate void CloseEvent();

    public event CloseEvent? Close;

    #endregion

    #region Constructors

    public DeleteEntryDialogView()
    {
        InitializeComponent();
    }

    #endregion

    #region Public methods

    public void SetEntryName(string name)
    {
        ViewModel!.Name = name;
    }

    #endregion

    #region Private methods

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    private void ButtonDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.DeleteEntry()) return;

            InvokeUi(() => Close?.Invoke());
        });
    }

    #endregion
}