using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;
using WinPass.UI.Models;

namespace WinPass.UI.ViewModels;

public class EntryListViewModel : ViewModelBase
{
    #region Props

    public EntryTreeViewModel EntryTreeViewModel { get; } = new();
    public ObservableCollection<EntryItemModel> Items { get; } = new();

    #endregion

    #region Constructors

    public EntryListViewModel()
    {
        RetrieveEntries();
    }

    #endregion

    #region Private methods

    private void RetrieveEntries()
    {
      Items.AddRange(new[]
        {
            new EntryItemModel { Name = "Test" },
            new EntryItemModel
            {
                Name = "Test",
                Items = new()
                {
                    new EntryItemModel
                    {
                        Name = "Sub Test"
                    }
                }
            },
            new EntryItemModel
            {
                Name = "Test",
                Items = new()
                {
                    new EntryItemModel
                    {
                        Name = "Sub Test",
                        Items = new()
                        {
                            new EntryItemModel
                            {
                                Name = "Sub sub Test",
                            }
                        }
                    }
                }
            },
            new EntryItemModel { Name = "Test" },
        });
    }

    #endregion
}