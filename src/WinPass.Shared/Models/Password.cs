namespace WinPass.Shared.Models;

public class Password
{
    public string Value { get; }
    public List<Metadata> Metadata { get; } = new();

    public Password(string value)
    {
        Value = value;
    }

    public Password(string value, List<Metadata> metadata)
    {
        Value = value;
        Metadata = metadata;
    }
}