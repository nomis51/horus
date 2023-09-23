namespace WinPass.Shared.Models.Display;

public class StoreEntry
{
    public string Name { get; }
    public List<StoreEntry> Entries { get; } = new();
    public bool Highlight { get; }
    public bool IsFolder { get; }
    public List<string> Metadata { get; } = new();

    public StoreEntry(string name, bool isFolder = false, bool highlight = false, List<string>? metadata = null)
    {
        Name = name;
        IsFolder = isFolder;
        Highlight = highlight;
        if (metadata is not null)
        {
            Metadata = metadata;
        }
    }
}