using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace WinPass.UI.Components.Abstractions;

public class Component : ComponentBase
{
    #region Services

    [Inject]
    protected ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    protected IJSRuntime JsRuntime { get; set; } = null!;

    [Inject]
    protected IDialogService DialogService { get; set; } = null!;

    #endregion
}