using Microsoft.AspNetCore.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class ConfirmDialogBase : Component
{
    #region Parameters

    [Parameter]
    public string ContentText { get; set; } = string.Empty;
    
    [Parameter]
    public EventCallback<bool> OnClose { get; set; }
    
    #endregion
}