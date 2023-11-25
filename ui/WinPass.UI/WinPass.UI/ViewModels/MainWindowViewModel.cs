using ReactiveUI;
using WinPass.UI.Models;

namespace WinPass.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Props

    public EntryListViewModel EntryListViewModel { get; set; } = new();
    public EntryFormViewModel EntryFormViewModel { get; set; } = new();

    #endregion
}