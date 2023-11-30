using System.Linq;
using System.Security.Cryptography.Xml;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Horus.UI.Models;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public partial class EntryListView : ViewBase<EntryListViewModel>
{
    #region Events

    public delegate void EntrySelectedEvent(string name);

    public event EntrySelectedEvent? EntrySelected;

    public delegate void CreateEntryEvent();

    public event CreateEntryEvent? CreateEntry;

    #endregion

    #region Constructors

    public EntryListView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    #endregion

    #region Public methods

    public void ReloadList()
    {
        Dispatch(vm => vm?.RetrieveEntries());
    }

    #endregion

    #region Private methods

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        TreeView.SelectedItem = TreeView.Items.FirstOrDefault();
    }

    private void TreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = e.AddedItems.Cast<EntryItemModel>().ElementAt(0);
        if (item.HasItems)
        {
            TreeView.SelectedItem = null;
            return;
        }

        EntrySelected?.Invoke(item.Name);
    }

    private void TextBoxSearch_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        Dispatch(vm => vm?.SearchEntries());
    }

    private void TextBoxSearch_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        InvokeUi(() => { TextBoxSearch.SelectAll(); });
    }


    private void EventListView_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.F)
        {
            InvokeUi(() => { TextBoxSearch.Focus(); });
        }
    }

    private void TextBoxSearch_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            TextBoxSearch.Clear();
        }

        if (e.Key == Key.Enter)
        {
            Dispatch(vm => { vm?.SearchEntries(e.KeyModifiers == KeyModifiers.Control); });
        }
    }

    private void ButtonCreateEntry_OnClick(object? sender, RoutedEventArgs e)
    {
        CreateEntry?.Invoke();
    }

    private void ButtonRefreshEntries_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (string.IsNullOrWhiteSpace(vm?.SearchText))
            {
                vm?.RetrieveEntries();
            }
            else
            {
                vm?.SearchEntries();
            }
        });
    }

    #endregion
}