using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Data;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class DuplicateEntryFormBase : Component
{
    #region Constants

    public const string DefaultEmptyPassword = "__empty__";

    #endregion

    #region Parameters

    [Parameter]
    public EventCallback<bool> OnClose { get; set; }

    [Parameter]
    public string ExistingName { get; set; }

    #endregion

    #region Members

    protected string Name { get; set; } = string.Empty;
    protected bool IsSaving { get; private set; }

    #endregion

    #region Protected methods

    protected void Close()
    {
        OnClose.InvokeAsync();
    }

    protected void Save()
    {
        IsSaving = true;

        Task.Run(async () =>
        {
            var result = AppService.Instance.RenameStoreEntry(ExistingName, Name, true);
            await InvokeAsync(() =>
            {
                IsSaving = false;
                StateHasChanged();
            });

            if (result.HasError)
            {
                Snackbar.Add($"Unable to duplicate store entry: {result.Error!.Message}", Severity.Error);
                return;
            }

            Snackbar.Add($"Entry '{ExistingName}' duplicated to '{Name}'", Severity.Success);
            await InvokeAsync(async () => { await OnClose.InvokeAsync(true); });
        });
    }

    #endregion
}