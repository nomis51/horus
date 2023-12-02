using Avalonia.Controls;
using Avalonia.Interactivity;
using Horus.UI.Abstractions;
using Horus.UI.Enums;
using Horus.UI.Services;
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
        var value = int.TryParse(TextBoxPasswordLength.Text, out var intValue) && intValue >= 0 ? intValue : 0;
        if (value == ViewModel!.Settings.DefaultLength) return;

        ViewModel!.Settings.DefaultLength = value;
        ViewModel?.PerformChanges();
    }

    private void TextBoxPasswordAlphabet_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var value = TextBoxPasswordAlphabet.Text ?? string.Empty;
        if (value == ViewModel!.Settings.DefaultCustomAlphabet) return;

        ViewModel!.Settings.DefaultCustomAlphabet = value;
        ViewModel?.PerformChanges();
    }

    private void TextBoxClearTimeout_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var value = int.TryParse(TextBoxClearTimeout.Text, out var intValue) && intValue >= 0 ? intValue : 0;
        if (value == ViewModel!.Settings.ClearTimeout) return;

        ViewModel!.Settings.ClearTimeout = value;
        ViewModel?.PerformChanges();
    }

    private void TextBoxAutoFetchInterval_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var value = int.TryParse(TextBoxAutoFetchInterval.Text, out var intValue) && intValue >= 0 ? intValue : 0;
        if (value == ViewModel!.Settings.FetchInterval) return;

        ViewModel!.Settings.FetchInterval = value;
        ViewModel?.PerformChanges();
    }

    private void ButtonDestroyStore_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.ConfirmDestroyStore();
    }

    #endregion
}