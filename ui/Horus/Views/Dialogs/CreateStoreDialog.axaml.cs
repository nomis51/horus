using Avalonia.Interactivity;
using Avalonia.Threading;
using Horus.Abstractions;
using Horus.Enums;
using Horus.ViewModels;

namespace Horus.Views.Dialogs;

public partial class CreateStoreDialog : DialogView<CreateStoreDialogViewModel>
{
    #region Constructors

    public CreateStoreDialog()
        : base(DialogType.CreateStore)
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
        OnClose(false);
    }

    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.CreateStore()) return;

            InvokeUi(() => OnClose(true));
        });
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose(false);
    }

    #endregion
}