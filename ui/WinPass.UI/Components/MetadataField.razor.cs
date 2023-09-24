using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WinPass.Shared.Enums;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class MetadataFieldBase : Component
{
    #region Parameters

    [Parameter]
    public string Key { get; set; } = string.Empty;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public MetadataType Type { get; set; } = MetadataType.Normal;

    [Parameter]
    public EventCallback<Tuple<string, string>> OnValueChanged { get; set; }

    #endregion

    #region Props

    protected bool IsInternal => Type == MetadataType.Internal;

    #endregion

    #region Protected methods

    protected async Task Update(string key, string value)
    {
        await OnValueChanged.InvokeAsync(Tuple.Create(key, value));
    }

    #endregion
}