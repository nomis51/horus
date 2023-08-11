using System.Collections;
using Newtonsoft.Json;
using WinPass.Shared.Extensions;

namespace WinPass.Shared.Models.Data;

public class MetadataCollection : ICollection<Metadata>
{
    private readonly List<Metadata> _entries = new();

    public string Name { get; }
    public int Count => _entries.Count;
    public bool IsReadOnly => false;

    public MetadataCollection()
    {
    }

    public MetadataCollection(string name, List<Metadata> entries)
    {
        Name = name;
        _entries = entries;
    }

    public Metadata this[int index]
    {
        get => _entries[index];
        set => _entries[index] = value;
    }

    public void RemoveAt(int index)
    {
        _entries.RemoveAt(index);
    }

    public int FindIndex(Func<Metadata, bool> fn)
    {
        for (var i = 0; i < _entries.Count; ++i)
        {
            if (fn(_entries[i])) return i;
        }

        return -1;
    }

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

    public override string ToString()
    {
        return JsonConvert.SerializeObject(_entries).ToBase64();
    }
}