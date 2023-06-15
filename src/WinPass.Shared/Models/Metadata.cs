namespace WinPass.Shared.Models;

public class Metadata
{
    public string Key { get; set; }
    public string Value { get;set; }

    public Metadata(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Key}: {Value}";
    }
}