using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using WinPass.Shared.Models.Display;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class EntriesTreeViewBase : Component
{
    #region Parameters

    [Parameter]
    public List<StoreEntry> Items { get; set; } = new();

    [Parameter]
    public string BasePath { get; set; } = string.Empty;
    
    [Parameter]
    public EventCallback<string> OnDeleteClick { get; set; }
    
    #endregion

    #region Protected methods

    protected string GetPath(StoreEntry item)
    {
        return string.IsNullOrEmpty(BasePath) ? item.Name : $"{BasePath}/{item.Name}";
    }

    #endregion
}