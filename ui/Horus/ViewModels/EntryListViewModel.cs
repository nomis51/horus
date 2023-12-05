using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Horus.Core.Services;
using Horus.Enums;
using Horus.Models;
using Horus.Services;
using Horus.Shared.Models.Display;
using ReactiveUI;
using Serilog;

namespace Horus.ViewModels;

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
            Log.Error("Failed to search store entries '{Text}': {Message}", SearchText, error.Message);
            SnackbarService.Instance.Show("Failed to search store entries", SnackbarSeverity.Warning, 5000);
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
            Log.Error("Failed to retrieve store entries: {Message}", error.Message);
            SnackbarService.Instance.Show("Failed to retrieve store entries", SnackbarSeverity.Error, 5000);
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
        return entries.Select(entry =>
            {
                var fullName = string.Join(
                    "/",
                    new[] { folder, entry.Name }
                        .Where(s => !string.IsNullOrEmpty(s))
                );
                return new EntryItemModel
                {
                    Name = entry.Name,
                    FullName = fullName,
                    Items = MapToEntryItemModels(entry.Entries, fullName),
                };
            })
            .ToList();
    }

    #endregion
}