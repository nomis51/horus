using System.Text;

namespace WinPass.Shared.Models.Data;

public class Settings
{
    public int DefaultLength { get; set; }
    public string DefaultCustomAlphabet { get; set; } = string.Empty;
    public int ClearTimeout { get; set; }
    public string Language { get; set; } = Locale.English;

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"{nameof(DefaultLength)}={DefaultLength}\n");
        sb.Append($"{nameof(DefaultCustomAlphabet)}={DefaultCustomAlphabet}\n");
        sb.Append($"{nameof(ClearTimeout)}={ClearTimeout}\n");
        sb.Append($"{nameof(Language)}={Language}\n");
        return sb.ToString();
    }
}