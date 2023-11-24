using Avalonia.Input;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Views;

public partial class EntryListView : ViewBase<EntryListViewModel>
{
    #region Constructors

    public EntryListView()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
    }

    #endregion
}