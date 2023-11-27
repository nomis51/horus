using Avalonia.Controls;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public class ViewBase<T> : UserControl
    where T : ViewModelBase
{
    #region Props

    protected T? ViewModel => DataContext as T;

    #endregion
}