using Newtonsoft.Json;
using WinPass.Shared.Enums;

namespace WinPass.Shared.Models;

public class Metadata
{
    [JsonIgnore]
    public MetadataType Type => TypeToString switch
    {
        "#" => MetadataType.Internal,
        _ => MetadataType.Normal,
    };

    public string Key { get; set; }
    public string Value { get; set; }

    public string TypeToString { get; set; }

    public Metadata(string key, string value, MetadataType type = MetadataType.Normal)
    {
        Key = key;
        Value = value;
        TypeToString = type switch
        {
            MetadataType.Internal => "#",
            _ => string.Empty,
        };
    }

    public override string ToString()
    {
        return $"{TypeToString}{Key}: {Value}";
    }
}