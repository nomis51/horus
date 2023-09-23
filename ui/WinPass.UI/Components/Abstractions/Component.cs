using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WinPass.UI.Components.Abstractions;

public class Component : ComponentBase
{
    #region Services

    [Inject]
    protected ISnackbar Snackbar { get; set; } = null!;

    #endregion
}