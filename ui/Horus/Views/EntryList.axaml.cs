using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Horus.Enums;
using Horus.Models;
using Horus.Services;
using Horus.ViewModels;

namespace Horus.Views;

public partial class EntryList : ViewBase<EntryListViewModel>
{
    #region Events

    public delegate void EntrySelectedEvent(string name);

    public event EntrySelectedEvent? EntrySelected;

    #endregion

    #region Constructors

    public EntryList()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    #endregion

    #region Public methods

    public void ReloadList(bool autoSelectFirst = false)
    {
        Dispatch(vm =>
        {
            vm?.RetrieveEntries();
            InvokeUi(() => { TreeView.SelectedItem = TreeView.Items.FirstOrDefault(); });
        });
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

        EntrySelected?.Invoke(item.FullName);
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
        DialogService.Instance.Show(DialogType.NewEntry);
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