using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Horus.Core.Services;
using Horus.Shared.Models.Display;
using Horus.UI.Models;
using Serilog;

namespace Horus.UI.ViewModels;

public class EntryListViewModel : ViewModelBase
{
    #region Props

    public ObservableCollection<EntryItemModel> Items { get; } = new();
    public string SearchText { get; set; } = string.Empty;

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
            Items.Clear();
            RetrieveEntries();
            return;
        }

        var (entries, error) = AppService.Instance.SearchStoreEntries(SearchText, searchMetadatas);

        if (error is not null)
        {
            Log.Error("Unable to search store entries: {Message}", error.Message);
            return;
        }

        Items.Clear();
        Items.AddRange(MapToEntryItemModels(entries));
    }

    #endregion

    #region Private methods

    private void RetrieveEntries()
    {
        var (entries, error) = AppService.Instance.GetStoreEntries();
        if (error is not null)
        {
            Log.Error("Unable to retrieve store entries: {Message}", error.Message);
            return;
        }

        Items.AddRange(MapToEntryItemModels(entries));
    }

    private List<EntryItemModel> MapToEntryItemModels(IEnumerable<StoreEntry> entries)
    {
        return entries.Select(entry => new EntryItemModel
            {
                Name = entry.Name,
                Items = MapToEntryItemModels(entry.Entries)
            })
            .ToList();
    }

    #endregion
}