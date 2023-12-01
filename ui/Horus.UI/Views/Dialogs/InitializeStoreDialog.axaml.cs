using Avalonia.Interactivity;
using Horus.UI.Abstractions;
using Horus.UI.ViewModels;

namespace Horus.UI.Views.Dialogs;

public partial class InitializeStoreDialog : DialogView<InitializeStoreDialogViewModel>
{
    #region Constructors

    public InitializeStoreDialog()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void ButtonQuit_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    #endregion
}