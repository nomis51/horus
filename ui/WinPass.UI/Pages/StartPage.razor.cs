using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.UI.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Pages;

public class StartPageBase : Component
{
    #region Services

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    #endregion

    #region Members

    private IDialogReference? _initStoreFormDialogReference;

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        EnsureStoreInitialized();
    }

    #endregion

    #region Private methods

    private void EnsureStoreInitialized()
    {
        if (!AppService.Instance.IsStoreInitialized())
        {
            _initStoreFormDialogReference = DialogService.Show<InitStoreForm>("Initialize store",
                new DialogParameters
                {
                    {
                        "OnClose",
                        EventCallback.Factory.Create(this, () =>
                        {
                            _initStoreFormDialogReference?.Close();
                            NavigationManager.NavigateTo("/home");
                        })
                    }
                }, new DialogOptions
                {
                    CloseButton = false,
                    CloseOnEscapeKey = false,
                    DisableBackdropClick = true,
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true
                });
        }
        else
        {
            NavigationManager.NavigateTo("/home");
        }
    }

    #endregion
}