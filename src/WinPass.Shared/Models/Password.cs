using Newtonsoft.Json;

namespace WinPass.Shared.Models;

public class Password : IDisposable
{
    public string Value { get; set; } = string.Empty;
    public List<Metadata> Metadata { get; set; } = new();

    public Password()
    {
    }

    public Password(string password)
    {
        Value = password;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    public void Dispose()
    {
        Value = string.Empty;
        GC.Collect();
    }
}