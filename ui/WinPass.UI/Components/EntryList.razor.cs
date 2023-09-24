using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Display;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class EntryListBase : Component
{
    #region Parameters

    [Parameter]
    public EventCallback<string> OnEntrySelected { get; set; }

    #endregion

    #region Members

    protected List<StoreEntry> Entries { get; private set; } = new();
    protected string SelectedEntry { get; set; } = string.Empty;
    protected string SearchText { get; private set; } = string.Empty;
    protected bool SearchInMetadata { get; set; }

    #endregion

    #region Lifecyle

    protected override void OnInitialized()
    {
        RetrieveEntries();
    }

    #endregion

    #region Protected methods

    protected void Search(string value)
    {
        SearchText = value;
        if (string.IsNullOrEmpty(SearchText))
        {
            RetrieveEntries();
            return;
        }

        Task.Run(async () =>
        {
            var (entries, error) = AppService.Instance.SearchStoreEntries(SearchText);
            if (error is not null)
            {
                Snackbar.Add("Unable to search store entries", Severity.Error);
                return;
            }

            Entries = entries;
            if (!string.IsNullOrEmpty(SelectedEntry)) return;

            await InvokeAsync(async () =>
            {
                await SelectEntry(Entries.FirstOrDefault(e => !e.IsFolder)?.Name ?? string.Empty);
                StateHasChanged();
            });
        });
    }

    protected async Task SelectEntry(string entry)
    {
        SelectedEntry = entry;
        await OnEntrySelected.InvokeAsync(entry);
    }

    #endregion


    #region Private methods

    private void RetrieveEntries()
    {
        Task.Run(async () =>
        {
            var (entries, error) = AppService.Instance.GetStoreEntries();
            if (error is not null)
            {
                Snackbar.Add("Unable to retrieve store entries", Severity.Error);
                return;
            }

            Entries = entries;
            await InvokeAsync(async () =>
            {
                await SelectEntry(Entries.FirstOrDefault(e => !e.IsFolder)?.Name ?? string.Empty);
                StateHasChanged();
            });
        });
    }

    #endregion
}