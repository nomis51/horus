using System;
using Horus.Shared.Enums;

namespace Horus.UI.Models;

public class MetadataModel
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public MetadataType Type { get; init; } = MetadataType.Normal;

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