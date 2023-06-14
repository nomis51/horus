namespace WinPass.Shared.Models.Fs;

public class StoreEntry
{
    public string Name { get; }
    public List<StoreEntry> Entries { get; } = new();
    public bool Highlight { get; }
    public bool IsFolder { get; }

    public StoreEntry(string name, bool isFolder = false, bool highlight = false)
    {
        Name = name;
        IsFolder = isFolder;
        Highlight = highlight;
    }
}