using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Data;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class SettingsFormBase : Component
{
    #region Constants

    protected readonly Dictionary<string, string> Languages = new()
    {
        { "en", "English" },
        { "fr", "French" },
        { "de", "German" },
    };

    #endregion

    #region Parameters

    [Parameter]
    public EventCallback OnClose { get; set; }

    #endregion

    #region Members

    protected Settings Settings { get; private set; }
    protected int GpgCacheTtl { get; set; }
    private IDialogReference? _destroyStoreConfirmDialog;

    #endregion

    #region Services

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        RetrieveSettings();
    }

    #endregion

    #region Protected methods

    protected void Save()
    {
        var result = AppService.Instance.SaveSettings(Settings);
        if (result.HasError)
        {
            Snackbar.Add($"Unable to save settings: {result.Error!.Message}", Severity.Error);
            return;
        }

        Snackbar.Add("Settings saved", Severity.Success);
        Close();
    }

    protected void ConfirmDestroyStore()
    {
        _destroyStoreConfirmDialog = DialogService.Show<ConfirmDialog>("Destroy the store", new DialogParameters
        {
            ["ContentText"] = "Are you sure you want to delete the password store?",
            ["OnClose"] = EventCallback.Factory.Create<bool>(this, confirmed =>
            {
                if (confirmed)
                {
                    if (!AppService.Instance.AcquireLock())
                    {
                        Snackbar.Add("Failed to acquire lock", Severity.Error);
                    }
                    else
                    {
                        var result = AppService.Instance.DestroyStore();
                        AppService.Instance.ReleaseLock();
                        if (!result.HasError)
                        {
                            Snackbar.Add("Store destroyed successfully", Severity.Success);
                            NavigationManager.NavigateTo("/");
                        }
                        else
                        {
                            Snackbar.Add($"Failed to destroy the store: {result.Error!.Message}", Severity.Error);
                        }
                    }
                }

                _destroyStoreConfirmDialog?.Close();
            }),
        }, new DialogOptions
        {
            CloseOnEscapeKey = true,
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        });
    }

    protected void Close()
    {
        OnClose.InvokeAsync();
    }

    #endregion

    #region Private methods

    private void RetrieveSettings()
    {
        var (settings, error) = AppService.Instance.GetSettings();
        if (error is not null || settings is null)
        {
            Snackbar.Add($"Unable to retrieve settings: {error!.Message}", Severity.Error);
            return;
        }

        Settings = settings;
    }

    #endregion
}