using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Serilog;
using WinPass.Core.Services;
using WinPass.Shared.Models.Display;
using WinPass.UI.Models;

namespace WinPass.UI.ViewModels;

public class EntryListViewModel : ViewModelBase
{
    #region Props

    public ObservableCollection<EntryItemModel> Items { get; } = new();

    #endregion

    #region Constructors

    public EntryListViewModel()
    {
        RetrieveEntries();
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