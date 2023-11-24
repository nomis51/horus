using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WinPass.UI.Models;

namespace WinPass.UI.ViewModels;

public class EntryTreeViewModel : ViewModelBase
{
    #region Props

    public ObservableCollection<EntryItemModel> Items { get; set; } = new();

    #endregion

    #region Public methods

    public void ToggleFolder(string name)
    {
        var item = Items.FirstOrDefault(i => i.Name == name);
        if (item is null || !item.HasSubItems) return;

        item.IsOpened = !item.IsOpened;
    }

    #endregion
}