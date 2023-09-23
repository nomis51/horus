using Microsoft.AspNetCore.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class EntryFormBase : Component
{
    #region Parameters

    [Parameter]
    public string SelectedEntry { get; set; } = string.Empty;

    #endregion
}