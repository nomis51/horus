using Avalonia.Interactivity;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public partial class SettingsView : ViewBase<SettingsViewModel>
{
    #region Events

    public delegate void CloseEvent();

    public event CloseEvent? Close;

    #endregion

    #region Constructors

    public SettingsView()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
    {
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    #endregion
}