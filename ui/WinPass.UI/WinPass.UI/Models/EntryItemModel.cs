using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Models;

public class EntryItemModel
{
    public string Name { get; set; }
    public List<EntryItemModel> Items { get; init; } = new();
    public bool HasItems => Items.Any();
}