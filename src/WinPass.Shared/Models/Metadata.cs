namespace WinPass.Shared.Models;

public class Metadata
{
    public string Key { get; }
    public string Value { get; }

    public Metadata(string key, string value)
    {
        Key = key;
        Value = value;
    }
}