using Avalonia.Controls;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Views;

public class ViewBase<T> : UserControl
    where T : ViewModelBase
{
    #region Props

    protected T? ViewModel => DataContext as T;

    #endregion
}