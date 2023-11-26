using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using WinPass.UI.Models;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Views;

public partial class EntryListView : ViewBase<EntryListViewModel>
{
    #region Commands

    public delegate void EntrySelectedEvent(string name);

    public event EntrySelectedEvent? OnEntrySelected;

    #endregion

    #region Constructors

    public EntryListView()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.AddedItems.Cast<EntryItemModel>().ElementAt(0);
        if (item.HasItems)
        {
            TreeView.SelectedItem = null;
            return;
        }

        OnEntrySelected?.Invoke(item.Name);
    }

    private void TextBoxSearch_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel?.SearchEntries();
    }

    private void TextBoxSearch_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { TextBoxSearch.SelectAll(); });
    }


    private void EventListView_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.F)
        {
            Dispatcher.UIThread.Invoke(() => { TextBoxSearch.Focus(); });
        }
    }

    private void TextBoxSearch_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            TextBoxSearch.Clear();
        }
    }

    #endregion
}