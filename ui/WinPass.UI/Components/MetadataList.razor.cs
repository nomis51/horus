using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class MetadataListBase : Component
{
    #region Parameters

    [Parameter]
    public string SelectedEntry { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<bool> AreMetadatasValid { get; set; }

    [Parameter]
    public EventCallback OnLoaded { get; set; }

    #endregion

    #region Members

    protected MetadataCollection Metadatas { get; private set; } = new();

    #endregion

    #region Lifecyle

    protected override void OnInitialized()
    {
        RetrieveMetadatas();
    }

    #endregion

    #region Public methods

    public EmptyResult Save()
    {
        return AppService.Instance.EditMetadatas(SelectedEntry, Metadatas);
    }

    #endregion

    #region Protected methods

    protected async Task UpdateMetadata(string key, string value, Metadata metadata)
    {
        metadata.Key = key;
        metadata.Value = value;
        await UpdateAreMetadatasValid();
    }


    #endregion

    #region Private methods

    private void RetrieveMetadatas()
    {
        if (string.IsNullOrEmpty(SelectedEntry)) return;

        Task.Run(async () =>
        {
            var (metadatas, error) = AppService.Instance.GetMetadatas(SelectedEntry);
            if (error is not null)
            {
                Snackbar.Add("Unable to retrieve entry metadata", Severity.Error);
                return;
            }

            Metadatas = metadatas;
            await InvokeAsync(async () =>
            {
                await OnLoaded.InvokeAsync();
                await UpdateAreMetadatasValid();
                StateHasChanged();
            });
        });
    }

    private async Task UpdateAreMetadatasValid()
    {
        await AreMetadatasValid.InvokeAsync(
            Metadatas.All(m =>
                !string.IsNullOrEmpty(m.Key) && !string.IsNullOrEmpty(m.Value)
            )
        );
    }

    #endregion
}