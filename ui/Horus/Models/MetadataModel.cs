using System;
using Horus.Shared.Enums;
using Horus.Shared.Models.Data;

namespace Horus.Models;

public class MetadataModel
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public MetadataType Type { get; init; } = MetadataType.Normal;

    public MetadataModel()
    {
    }

    public MetadataModel(Metadata metadata)
    {
        Key = metadata.Key;
        Value = metadata.Value;
        Type = metadata.Type;
    }

    public MetadataModel(string key, string value, MetadataType type)
    {
        Key = key;
        Value = value;
        Type = type;
    }

    public string DisplayValue
    {
        get
        {
            if (Type == MetadataType.HistoryDate && Key is "created" or "modified" && DateTime.TryParse(Value, out var date))
            {
                return date.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return Value;
        }
    }

    public void Clear()
    {
        Value = string.Empty;
    }
}