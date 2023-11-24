using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class InitStoreFormBase : Component
{
    #region Parameters

    [Parameter]
    public EventCallback OnClose { get; set; }

    #endregion

    #region Members

    protected string GpgId { get; private set; } = string.Empty;
    protected string GitUrl { get; private set; } = string.Empty;
    protected bool IsGpgIdValid { get; private set; } = true;
    protected bool IsLoading { get; private set; }

    #endregion

    #region Protected methods

    protected void Create()
    {
        IsLoading = true;
        Task.Run(async () =>
        {
            var result = AppService.Instance.InitializeStoreFolder(GpgId, GitUrl);
            if (result.HasError)
            {
                Snackbar.Add($"Failed to initialize store: {result.Error!.Message}", Severity.Error);
                return;
            }

            Snackbar.Add("Store initialized successfully", Severity.Success);

            await InvokeAsync(async () =>
            {
                IsLoading = false;
                await OnClose.InvokeAsync();
            });
        });
    }

    protected void Quit()
    {
        Environment.Exit(0);
    }

    protected void SetGpgId(string value)
    {
        GpgId = value;
    }

    protected void SetGitHubUrl(string value)
    {
        GitUrl = value;
    }

    #endregion
}