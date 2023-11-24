using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.UI.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Pages;

public class StartPageBase : Component, IDisposable
{
    #region Services

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    #endregion

    #region Members

    private IDialogReference? _initStoreFormDialogReference;

    #endregion

    #region Members

    private Task? _checkTask;
    private bool _doCheckStore = true;

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        AutoCheckStore();
    }

    public void Dispose()
    {
        _doCheckStore = false;
    }

    #endregion

    #region Private methods

    private void EnsureStoreInitialized()
    {
        if (_initStoreFormDialogReference is not null) return;
        
        if (!AppService.Instance.IsStoreInitialized())
        {
            InvokeAsync(() =>
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
            });
        }
        else
        {
            NavigationManager.NavigateTo("/home");
        }
    }

    private void AutoCheckStore()
    {
        _checkTask = Task.Run(() =>
        {
            while (_doCheckStore)
            {
                EnsureStoreInitialized();
                Thread.Sleep(1000);
            }
        });
    }

    #endregion
}