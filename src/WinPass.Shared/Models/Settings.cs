using Newtonsoft.Json;

namespace WinPass.Shared.Models;

public class Settings
{
    public int DefaultLength { get; set; }
    public string DefaultCustomAlphabet { get; set; }
    public int ClearTimeout { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}