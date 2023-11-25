using System;
using Humanizer;
using WinPass.Shared.Enums;

namespace WinPass.UI.Models;

public class MetadataModel
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; }= string.Empty;
    public MetadataType Type { get; set; } = MetadataType.Normal;
    public string DisplayKey => Key.Humanize();

    public string DisplayValue
    {
        get
        {
            if (Type == MetadataType.Internal && Key is "created" or "modified" && DateTime.TryParse(Value, out var date))
            {
                return date.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return Value;
        }
    }
}