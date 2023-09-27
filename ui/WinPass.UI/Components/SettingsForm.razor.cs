using Microsoft.AspNetCore.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class SettingsFormBase : Component
{
    #region Constants

    protected readonly string[] Languages = new[]
    {
        "English",
        "French",
        "German",
    };

    #endregion

    #region Parameters

    [Parameter]
    public EventCallback OnClose { get; set; }

    #endregion
}