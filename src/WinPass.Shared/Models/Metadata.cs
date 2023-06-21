using WinPass.Shared.Enums;

namespace WinPass.Shared.Models;

public class Metadata
{
    public MetadataType Type { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }

    public Metadata(string key, string value, MetadataType type = MetadataType.Normal)
    {
        Key = key;
        Value = value;
        Type = type;
    }

    public override string ToString()
    {
        return $"{TypeToString()}{Key}: {Value}";
    }

    private string TypeToString()
    {
        return Type switch
        {
            MetadataType.Internal => "#",
            _ => string.Empty
        };
    }
}