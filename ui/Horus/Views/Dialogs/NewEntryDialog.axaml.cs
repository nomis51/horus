using Avalonia.Interactivity;
using Avalonia.Threading;
using Horus.Abstractions;
using Horus.Enums;
using Horus.ViewModels;

namespace Horus.Views.Dialogs;

public partial class NewEntryDialog : DialogView<NewEntryDialogViewModel>
{
    #region Constructors

    public NewEntryDialog()
        : base(DialogType.NewEntry)
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
        ViewModel!.Name = string.Empty;
        OnClose();
    }

    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.CreateEntry()) return;

            InvokeUi(() =>
            {
                OnClose(vm.Name);
                vm.Name = string.Empty;
            });
        });
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel!.Name = string.Empty;
        OnClose();
    }

    #endregion
}