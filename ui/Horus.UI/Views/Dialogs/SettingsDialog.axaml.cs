using Avalonia.Controls;
using Avalonia.Interactivity;
using Horus.UI.Abstractions;
using Horus.UI.ViewModels;

namespace Horus.UI.Views.Dialogs;

public partial class SettingsDialog : DialogView<SettingsDialogViewModel>
{
    #region Constructors

    public SettingsDialog()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose();
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.SaveSettings()) return;

            InvokeUi(() => OnClose());
        });
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose();
    }

    private void TextBoxPasswordLength_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel!.Settings.DefaultLength = int.TryParse(TextBoxPasswordLength.Text, out var intValue) && intValue >= 0 ? intValue : 0;
    }

    private void TextBoxPasswordAlphabet_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel!.Settings.DefaultCustomAlphabet = TextBoxPasswordAlphabet.Text ?? string.Empty;
    }

    private void TextBoxClearTimeout_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel!.Settings.ClearTimeout = int.TryParse(TextBoxClearTimeout.Text, out var intValue) && intValue >= 0 ? intValue : 0;
    }

    private void TextBoxAutoFetchInterval_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel!.Settings.FetchInterval = int.TryParse(TextBoxAutoFetchInterval.Text, out var intValue) && intValue >= 0 ? intValue : 0;
    }

    #endregion
}