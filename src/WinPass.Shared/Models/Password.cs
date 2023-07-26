using System.Text;
using Newtonsoft.Json;

namespace WinPass.Shared.Models;

public class Password : IDisposable
{
    public string? Value
    {
        get => ValueBytes is null ? null : Encoding.UTF8.GetString(ValueBytes);
        set => ValueBytes = value is null ? null : Encoding.UTF8.GetBytes(value);
    }

    [JsonIgnore]
    public byte[]? ValueBytes { get; set; }

    [JsonIgnore]
    public string ValueAsString => ValueBytes is null ? string.Empty : Encoding.UTF8.GetString(ValueBytes);

    public List<Metadata> Metadata { get; init; } = new();

    public Password()
    {
    }

    public Password(ref string password)
    {
        ValueBytes = Encoding.UTF8.GetBytes(password);
    }

    public void SetValue(ref string value)
    {
        ValueBytes = Encoding.UTF8.GetBytes(value);
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public void Dispose()
    {
        ValueBytes = null;
        GC.Collect();
    }
}