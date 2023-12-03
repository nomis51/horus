using Horus.Core.Services;
using ReactiveUI;

namespace Horus.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Props

    public EntryListViewModel EntryListViewModel { get; set; } = new();
    public EntryFormViewModel EntryFormViewModel { get; set; } = new();
    public TitleBarViewModel TitleBarViewModel { get; set; } = new();
    public DialogManagerViewModel DialogManagerViewModel { get; set; } = new();
    public SnackbarManagerViewModel SnackbarManagerViewModel { get; set; } = new();

    private bool _isLoading = true;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private string _loadingMessage = string.Empty;

    public string LoadingMessage
    {
        get => _loadingMessage;
        set => this.RaiseAndSetIfChanged(ref _loadingMessage, value);
    }

    private bool _entrySelected;

    public bool EntrySelected
    {
        get => _entrySelected;
        set => this.RaiseAndSetIfChanged(ref _entrySelected, value);
    }

    #endregion

    #region Public methods

    public bool IsStoreInitialized()
    {
        return AppService.Instance.IsStoreInitialized();
    }

    #endregion
}