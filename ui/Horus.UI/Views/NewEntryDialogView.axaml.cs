using Avalonia.Interactivity;
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
    }

    #endregion

    #region Private methods

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke(ViewModel!.Name);
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke();
    }

    #endregion
}