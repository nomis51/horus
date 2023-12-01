using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Horus.Core.Services;
using Horus.Shared.Models.Display;
using Horus.UI.Models;
using ReactiveUI;
using Serilog;

namespace Horus.UI.ViewModels;

public class EntryListViewModel : ViewModelBase
{
    #region Props

    public ObservableCollection<EntryItemModel> Items { get; } = new();
    public string SearchText { get; set; } = string.Empty;

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    #endregion

    #region Constructors

    public EntryListViewModel()
    {
        RetrieveEntries();
    }

    #endregion

    #region Public methods

    public void SearchEntries(bool searchMetadatas = false)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            RetrieveEntries();
            return;
        }

        IsLoading = true;
        var (entries, error) = AppService.Instance.SearchStoreEntries(SearchText, searchMetadatas);
        IsLoading = false;

        if (error is not null)
        {
            Log.Error("Unable to search store entries: {Message}", error.Message);
            return;
        }

        InvokeUi(() =>
        {
            Items.Clear();
            Items.AddRange(MapToEntryItemModels(entries));
        });
    }

    public void RetrieveEntries()
    {
        IsLoading = true;
        var (entries, error) = AppService.Instance.GetStoreEntries();
        IsLoading = false;

        if (error is not null)
        {
            Log.Error("Unable to retrieve store entries: {Message}", error.Message);
            return;
        }

        InvokeUi(() =>
        {
            Items.Clear();
            Items.AddRange(MapToEntryItemModels(entries));
        });
    }

    #endregion

    #region Private methods

    private List<EntryItemModel> MapToEntryItemModels(IEnumerable<StoreEntry> entries, string folder = "")
    {
        return entries.Select(entry => new EntryItemModel
            {
                Name = entry.Name,
                FullName = string.Join(
                    "/",
                    new[] { folder, entry.Name }
                        .Where(s => !string.IsNullOrEmpty(s))
                ),
                Items = MapToEntryItemModels(entry.Entries, entry.Name),
            })
            .ToList();
    }

    #endregion
}