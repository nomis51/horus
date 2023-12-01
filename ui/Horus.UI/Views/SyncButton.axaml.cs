using Avalonia.Interactivity;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public partial class SyncButton : ViewBase<SyncButtonViewModel>
{
    #region Constructors

    public SyncButton()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ButtonSync_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.Sync());
    }

    #endregion
}