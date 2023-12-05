using System.Text.Json;

namespace Horus.Shared.Models.Data;

public class Settings
{
    public int DefaultLength { get; set; }
    public string DefaultCustomAlphabet { get; set; } = string.Empty;
    public int ClearTimeout { get; set; }
    public string Language { get; set; } = Locale.English;
    public int FetchInterval { get; set; }
    public string AppDataLocation { get; set; } = string.Empty;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}