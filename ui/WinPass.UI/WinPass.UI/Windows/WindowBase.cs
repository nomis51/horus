using Avalonia.Controls;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Windows;

public class WindowBase<T> : Window
where T : ViewModelBase
{
    #region Props

    protected T? ViewModel => DataContext as T;

    #endregion
}