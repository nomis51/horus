using Avalonia.Interactivity;
using Horus.UI.Abstractions;
using Horus.UI.ViewModels;

namespace Horus.UI.Views.Dialogs;

public partial class SettingsDialog : DialogView<SettingsViewModel>
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
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose();
    }

    #endregion
}