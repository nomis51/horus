using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class TreeViewBase<T> : Component
    where T : class
{
    #region Parameters

    [Parameter]
    public List<T> Items { get; set; } = new();

    [Parameter]
    public RenderFragment<T> ChildContent { get; set; }

    [Parameter]
    public Func<T, bool> IsBranch { get; set; }

    [Parameter]
    public Func<T, List<T>> GetChildItems { get; set; }

    [Parameter]
    public Func<T, string> GetText { get; set; }

    [Parameter]
    public string BasePath { get; set; } = string.Empty;

    #endregion

    #region Protected methods

    protected string GetPath(T item)
    {
        return string.IsNullOrEmpty(BasePath) ? GetText.Invoke(item) : $"{BasePath}/{GetText.Invoke(item)}";
    }

    #endregion
}