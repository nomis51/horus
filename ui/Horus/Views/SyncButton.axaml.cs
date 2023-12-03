using Avalonia.Interactivity;
using Horus.ViewModels;

namespace Horus.Views;

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