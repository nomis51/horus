using System.Linq;
using Avalonia.Controls;
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

    #endregion
}