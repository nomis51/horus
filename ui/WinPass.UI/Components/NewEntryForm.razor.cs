﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Data;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class NewEntryFormBase : Component
{
    #region Constants

    public const string DefaultEmptyPassword = "__empty__";

    #endregion

    #region Parameters

    [Parameter]
    public EventCallback<bool> OnClose { get; set; }

    #endregion

    #region Members

    protected string Name { get; set; } = string.Empty;
    protected bool IsSaving { get; private set; }
    protected bool IsValid { get; private set; }

    #endregion

    #region Protected methods

    protected void UpdateName(string value)
    {
        Name = value;
        IsValid = !AppService.Instance.DoStoreEntryExists(Name);
        StateHasChanged();
    }

    protected void Close()
    {
        OnClose.InvokeAsync();
    }

    protected void Save()
    {
        IsSaving = true;

        Task.Run(async () =>
        {
            var result = AppService.Instance.InsertPassword(Name, new Password(DefaultEmptyPassword));
            await InvokeAsync(() =>
            {
                IsSaving = false;
                StateHasChanged();
            });

            if (result.HasError)
            {
                Snackbar.Add($"Unable to add new store entry: {result.Error!.Message}", Severity.Error);
                return;
            }

            Snackbar.Add($"Entry '{Name}' created", Severity.Success);
            await InvokeAsync(async () => { await OnClose.InvokeAsync(true); });
        });
    }

    #endregion
}