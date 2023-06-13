namespace WinPass.Shared.Models.Fs;

public class StoreEntry
{
    public string Name { get; }
    public List<StoreEntry> Entries { get; } = new();

    public StoreEntry(string name)
    {
        Name = name;
    }
}