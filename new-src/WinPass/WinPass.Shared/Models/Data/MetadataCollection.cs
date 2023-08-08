using System.Collections;

namespace WinPass.Shared.Models.Data;

public class MetadataCollection : ICollection<Metadata>
{
    private readonly List<Metadata> _entries = new();

    public int Count => _entries.Count;
    public bool IsReadOnly => false;

    public IEnumerator<Metadata> GetEnumerator()
    {
        return _entries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(Metadata item)
    {
        _entries.Add(item);
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public bool Contains(Metadata item)
    {
        return _entries.Contains(item);
    }

    public void CopyTo(Metadata[] array, int arrayIndex)
    {
        _entries.CopyTo(array, arrayIndex);
    }

    public bool Remove(Metadata item)
    {
        return _entries.Remove(item);
    }
}