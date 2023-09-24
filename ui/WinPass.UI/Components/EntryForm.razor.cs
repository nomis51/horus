using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Data;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class EntryFormBase : Component
{
    #region Parameters

    [Parameter]
    public string SelectedEntry { get; set; } = string.Empty;

    #endregion

    #region Members

    protected bool IsPasswordReadOnly { get; private set; } = true;
    protected bool IsPasswordValid { get; set; }
    protected bool AreMetadatasReadOnly { get; private set; } = true;
    protected MetadataCollection Metadatas { get; private set; } = new();

    #endregion

    #region Protected methods

    protected void EditPassword()
    {
        IsPasswordValid = false;
        IsPasswordReadOnly = false;
    }

    protected void CancelPassword()
    {
        IsPasswordValid = false;
        IsPasswordReadOnly = true;
    }

    protected void SavePassword()
    {
        // TODO: 
        CancelPassword();
    }

    protected void EditMetadatas()
    {
        AreMetadatasReadOnly = false;
        Task.Run(async () =>
        {
            var (metadatas, error) = AppService.Instance.GetMetadatas(SelectedEntry);
            if (error is not null)
            {
                Snackbar.Add("Unable to retrieve entry metadata", Severity.Error);
                return;
            }

            Metadatas = metadatas;
            await InvokeAsync(StateHasChanged);
        });
    }

    protected void CancelMetadatas()
    {
        AreMetadatasReadOnly = true;
    }

    protected void SaveMetadatas()
    {
        // TODO: 
        CancelMetadatas();
    }

    #endregion
}