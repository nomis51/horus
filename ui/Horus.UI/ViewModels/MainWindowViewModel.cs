using ReactiveUI;

namespace Horus.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Props

    public EntryListViewModel EntryListViewModel { get; set; } = new();
    public EntryFormViewModel EntryFormViewModel { get; set; } = new();
    public TitleBarViewModel TitleBarViewModel { get; set; } = new();
    public DialogManagerViewModel DialogManagerViewModel { get; set; } = new();
    public SnackbarManagerViewModel SnackbarManagerViewModel { get; set; } = new();

    private bool _entrySelected;

    public bool EntrySelected
    {
        get => _entrySelected;
        set => this.RaiseAndSetIfChanged(ref _entrySelected, value);
    }

    #endregion
}