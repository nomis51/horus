using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using WinPass.UI.Components;
using Page = WinPass.UI.Components.Abstractions.Page;

namespace WinPass.UI.Pages;

public class HomePageBase : Page, IDisposable
{
    #region Events

    private delegate void OpenSettingsEvent();

    private static event OpenSettingsEvent OnOpenSettings = null!;
    private static void OpenSettingsEventInvoke() => OnOpenSettings?.Invoke();

    #endregion

    #region Members

    protected string SelectedEntry { get; set; } = string.Empty;
    protected Sidenav? SidenavRef { get; set; }
    private IDialogReference? _settingsFormDialogReference;

    #endregion

    #region Lifecyle

    protected override void OnInitialized()
    {
        OnOpenSettings += OpenSettings;
    }

    public void Dispose()
    {
        OnOpenSettings -= OpenSettings;
    }

    #endregion

    #region JS invokables

    [JSInvokable("js-open-settings")]
    public static void OnJsOpenSettingsForm()
    {
        OpenSettingsEventInvoke();
    }

    #endregion

    #region Protected methods

    protected async Task SelectEntry(string entry)
    {
        SelectedEntry = entry;
        await InvokeAsync(StateHasChanged);
    }

    protected async Task RefreshEntries()
    {
        await SidenavRef!.RefreshEntries();
    }

    #endregion

    #region Private methods

    private void OpenSettings()
    {
        _settingsFormDialogReference = DialogService.Show<SettingsForm>("Settings",
            new DialogParameters
            {
                {
                    "OnClose",
                    EventCallback.Factory.Create(this, () => { _settingsFormDialogReference?.Close(); })
                }
            }, new DialogOptions
            {
                CloseButton = true,
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Medium,
                FullWidth = true
            });
    }

    #endregion
}