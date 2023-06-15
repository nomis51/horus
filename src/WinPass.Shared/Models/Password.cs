using System.Text;

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

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine(Value);
        
        foreach (var metadata in Metadata)
        {
            sb.AppendLine(metadata.ToString());
        }

        return sb.ToString();
    }
}