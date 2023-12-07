using System.Collections.Generic;
using Material.Icons;

namespace Horus.Models;

public class EntryItemModel
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public List<EntryItemModel> Items { get; init; } = new();
    public bool HasItems => Items.Count != 0;
    public MaterialIconKind Icon { get; set; } = MaterialIconKind.Key;
}