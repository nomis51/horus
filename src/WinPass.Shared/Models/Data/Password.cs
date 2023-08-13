using System.Text;
using Newtonsoft.Json;
using WinPass.Shared.Extensions;

namespace WinPass.Shared.Models.Data;

public class Password : IDisposable
{
    public byte[]? Value { get; private set; }

    public string ValueAsString => Value is null || Value.Length == 0 ? string.Empty : Encoding.UTF8.GetString(Value);

    public Password(string value)
    {
        Value = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(value);
    }

    public Password(byte[] value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return ValueAsString.ToBase64();
    }

    public void Dispose()
    {
        Value = null;
        GC.Collect();
    }
}