using Horus.Shared.Enums;

namespace Horus.Shared.Models.Data;

public class Metadata
{
    public string Key { get; set; }
    public string Value { get; set; }
    public MetadataType Type { get; set; }

    public Metadata()
    {
    }

    public Metadata(string key, string value, MetadataType type = MetadataType.Normal)
    {
        Key = key;
        Value = value;
        Type = type;
    }

    public override string ToString()
    {
        return $"{Key}: {Value}";
    }
}