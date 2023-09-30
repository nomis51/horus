using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Data;
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
    private string SelectedEntry { get; set; } = string.Empty;
    protected string SearchText { get; private set; } = string.Empty;
    protected bool SearchInMetadata { get; set; }
    private IDialogReference? _addNewEntryDialogReference;
    private IDialogReference? _duplicateEntryDialogReference;

    #endregion

    #region Lifecyle

    protected override void OnInitialized()
    {
        RetrieveEntries();
    }

    #endregion

    #region Public methods

    public Task RefreshEntries()
    {
        return RetrieveEntries();
    }

    #endregion

    #region Protected methods

    protected void DuplicateEntry(string name)
    {
        _duplicateEntryDialogReference = DialogService.Show<DuplicateEntryForm>("Duplicate store entry",
            new DialogParameters
            {
                { "ExistingName", name },
                {
                    "OnClose", EventCallback.Factory.Create(this, (bool reload) =>
                    {
                        if (!reload)
                        {
                            _duplicateEntryDialogReference?.Close();
                            return;
                        }

                        _duplicateEntryDialogReference?.Close();
                        RetrieveEntries();
                    })
                }
            }, new DialogOptions
            {
                CloseButton = true,
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            });
    }

    protected void DeleteEntry(string name)
    {
        Task.Run(() =>
        {
            if (!AppService.Instance.AcquireLock())
            {
                Snackbar.Add($"Unable to acquire store lock", Severity.Error);
                return;
            }

            var result = AppService.Instance.DeleteStoreEntry(name);
            AppService.Instance.ReleaseLock();
            if (result.HasError)
            {
                Snackbar.Add($"Unable to remove the store entry: {result.Error!.Message}", Severity.Error);
                return;
            }

            Snackbar.Add("Store entry removed", Severity.Success);
            RetrieveEntries();
        });
    }

    protected void ShowAddEntryForm()
    {
        _addNewEntryDialogReference = DialogService.Show<NewEntryForm>("Add new store entry", new DialogParameters
        {
            {
                "OnClose", EventCallback.Factory.Create(this, (bool reload) =>
                {
                    if (!reload)
                    {
                        _addNewEntryDialogReference?.Close();
                        return;
                    }

                    _addNewEntryDialogReference?.Close();
                    RetrieveEntries();
                })
            }
        }, new DialogOptions
        {
            CloseButton = true,
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        });
    }

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

    private Task RetrieveEntries()
    {
        return Task.Run(async () =>
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