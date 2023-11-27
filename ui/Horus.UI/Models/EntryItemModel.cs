using System.Collections.Generic;
using System.Linq;

namespace Horus.UI.Models;

public class EntryItemModel
{
    public string Name { get; set; }
    public List<EntryItemModel> Items { get; init; } = new();
    public bool HasItems => Items.Any();
}