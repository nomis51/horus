using Avalonia.Interactivity;
using Horus.UI.Abstractions;
using Horus.UI.ViewModels;

namespace Horus.UI.Views.Dialogs;

public partial class DeleteEntryDialog : DialogView<DeleteEntryDialogViewModel>
{
    #region Constructors

    public DeleteEntryDialog()
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
        OnClose();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose();
    }

    private void ButtonDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.DeleteEntry()) return;

            InvokeUi(() => OnClose());
        });
    }

    #endregion
}