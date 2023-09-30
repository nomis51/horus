using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class EntryFormBase : Component
{
    #region Parameters

    [Parameter]
    public string SelectedEntry { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnRefreshEntries { get; set; }

    #endregion

    #region Members

    protected PasswordField? PasswordFieldRef { get; set; }
    protected MetadataList? MetadataListRef { get; set; }
    protected bool IsPasswordReadOnly { get; private set; } = true;
    protected bool IsPasswordValid { get; set; }
    protected bool AreMetadatasReadOnly { get; private set; } = true;
    protected bool AreMetadatasValid { get; set; }
    protected bool IsSaving { get; private set; }
    protected bool IsLoadingMetadatas { get; private set; }
    protected string NewEntryName { get; set; } = string.Empty;
    protected bool IsEditingEntryName { get; set; }

    #endregion


    #region Protected methods

    protected void EditEntryName()
    {
        NewEntryName = SelectedEntry;
        IsEditingEntryName = true;
    }

    protected void CancelEntryName()
    {
        IsEditingEntryName = false;
    }

    protected void SaveEntryName()
    {
        Task.Run(async () =>
        {
            var result = AppService.Instance.RenameStoreEntry(SelectedEntry, NewEntryName);
            if (result.HasError)
            {
                Snackbar.Add($"Unable to rename entry: {result.Error!.Message}", Severity.Error);
                return;
            }

            await InvokeAsync(async () =>
            {
                await OnRefreshEntries.InvokeAsync();
                SelectedEntry = NewEntryName;
                CancelEntryName();
                StateHasChanged();
            });

            Snackbar.Add("Entry renamed", Severity.Success);
        });
    }

    protected void GenerateNewPassword()
    {
        PasswordFieldRef?.GeneratePassword();
    }

    protected void EditPassword()
    {
        IsPasswordValid = false;
        IsPasswordReadOnly = false;
    }

    protected async Task CancelPassword()
    {
        await InvokeAsync(() =>
        {
            IsSaving = false;
            IsPasswordValid = false;
            IsPasswordReadOnly = true;
            StateHasChanged();
        });
    }

    protected async Task SavePassword()
    {
        await InvokeAsync(() => { IsSaving = true; });

        _ = Task.Run(async () =>
        {
            var result = PasswordFieldRef!.Save();
            if (result.HasError)
            {
                Snackbar.Add($"Unable to save password: {result.Error!.Message}", Severity.Error);
                return;
            }

            Snackbar.Add("Password saved", Severity.Success);
            await CancelPassword();
        });
    }

    protected async Task OnMetadataLoaded()
    {
        await InvokeAsync(() => { IsLoadingMetadatas = false; });
    }

    protected void EditMetadatas()
    {
        IsLoadingMetadatas = true;
        AreMetadatasReadOnly = false;
    }

    protected async Task CancelMetadatas()
    {
        await InvokeAsync(() =>
        {
            IsSaving = false;
            AreMetadatasReadOnly = true;
            StateHasChanged();
        });
    }

    protected async Task SaveMetadatas()
    {
        await InvokeAsync(() => { IsSaving = true; });

        _ = Task.Run(async () =>
        {
            var result = MetadataListRef!.Save();
            if (result.HasError)
            {
                Snackbar.Add($"Unable to save metadata: {result.Error!.Message}", Severity.Error);
                return;
            }

            Snackbar.Add("Metadata saved", Severity.Success);
            await CancelMetadatas();
        });
    }

    #endregion
}