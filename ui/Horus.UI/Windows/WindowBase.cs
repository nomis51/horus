using Avalonia.Controls;
using Horus.UI.ViewModels;

namespace Horus.UI.Windows;

public class WindowBase<T> : Window
where T : ViewModelBase
{
    #region Props

    protected T? ViewModel => DataContext as T;

    #endregion
}