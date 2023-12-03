using Avalonia.Interactivity;
using Horus.Abstractions;
using Horus.Enums;
using Horus.ViewModels;

namespace Horus.Views.Dialogs;

public partial class DeleteEntryDialog : DialogView<DeleteEntryDialogViewModel>
{
    #region Constructors

    public DeleteEntryDialog()
        : base(DialogType.DeleteEntry)
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
        OnClose(false);
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose(false);
    }

    private void ButtonDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.DeleteEntry()) return;

            InvokeUi(() => OnClose(true));
        });
    }

    #endregion
}