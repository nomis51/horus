using Microsoft.AspNetCore.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class SidenavBase : Component
{
    #region Parameters

    [Parameter]
    public EventCallback<string> OnEntrySelected { get; set; }

    #endregion
}