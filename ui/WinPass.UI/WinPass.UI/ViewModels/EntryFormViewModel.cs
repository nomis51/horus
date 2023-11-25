using ReactiveUI;
using WinPass.UI.Models;

namespace WinPass.UI.ViewModels;

public class EntryFormViewModel : ViewModelBase
{
    #region Props

    public string EntryName { get; set; } = string.Empty;
    public bool HasEntry => !string.IsNullOrWhiteSpace(EntryName);

    #endregion

    #region Public methods

    public void SetEntryItem(string name)
    {
        EntryName = name;
        this.RaisePropertyChanged(nameof(EntryName));
    }

    #endregion
}