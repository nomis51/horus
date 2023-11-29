using System.Threading.Tasks;
using Avalonia.Controls.Chrome;
using Avalonia.Threading;
using Horus.Core.Services;
using Horus.Shared.Models.Data;
using Horus.UI.Services;
using ReactiveUI;

namespace Horus.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Props

    public EntryListViewModel EntryListViewModel { get; set; } = new();
    public EntryFormViewModel EntryFormViewModel { get; set; } = new();
    public TitleBarViewModel TitleBarViewModel { get; set; } = new();
    public NewEntryDialogViewModel NewEntryDialogViewModel { get; set; } = new();
    public SettingsViewModel SettingsViewModel { get; set; } = new();

    private bool _entrySelected;

    public bool EntrySelected
    {
        get => _entrySelected;
        set => this.RaiseAndSetIfChanged(ref _entrySelected, value);
    }

    private string _snackbarText = string.Empty;

    public string SnackbarText
    {
        get => _snackbarText;
        set => this.RaiseAndSetIfChanged(ref _snackbarText, value);
    }

    private bool _isSnackbarVisible;

    public bool IsSnackbarVisible
    {
        get => _isSnackbarVisible;
        set => this.RaiseAndSetIfChanged(ref _isSnackbarVisible, value);
    }

    private bool _snackbarSeverityAccent;

    public bool SnackbarSeverityAccent
    {
        get => _snackbarSeverityAccent;
        set => this.RaiseAndSetIfChanged(ref _snackbarSeverityAccent, value);
    }

    private bool _snackbarSeverityError;

    public bool SnackbarSeverityError
    {
        get => _snackbarSeverityError;
        set => this.RaiseAndSetIfChanged(ref _snackbarSeverityError, value);
    }

    private bool _snackbarSeveritySuccess;

    public bool SnackbarSeveritySuccess
    {
        get => _snackbarSeveritySuccess;
        set => this.RaiseAndSetIfChanged(ref _snackbarSeveritySuccess, value);
    }

    private bool _snackbarSeverityWarning;

    public bool SnackbarSeverityWarning
    {
        get => _snackbarSeverityWarning;
        set => this.RaiseAndSetIfChanged(ref _snackbarSeverityWarning, value);
    }

    private bool _isNewEntryDialogVisible;

    public bool IsNewEntryDialogVisible
    {
        get => _isNewEntryDialogVisible;
        set => this.RaiseAndSetIfChanged(ref _isNewEntryDialogVisible, value);
    }

    private bool _isSettingsDialogVisible;

    public bool IsSettingsDialogVisible
    {
        get => _isSettingsDialogVisible;
        set => this.RaiseAndSetIfChanged(ref _isSettingsDialogVisible, value);
    }

    #endregion

    #region Constructors

    public MainWindowViewModel()
    {
        SnackbarService.Instance.OnShow += Snackbar_OnShow;
    }

    #endregion

    #region Public methods

    public void CloseSettingsDialog()
    {
        IsSettingsDialogVisible = false;
    }

    public void OpenSettingsDialog()
    {
        IsSettingsDialogVisible = true;
    }

    public void OpenNewEntryDialog()
    {
        IsNewEntryDialogVisible = true;
    }

    public void CloseNewEntryDialog()
    {
        IsNewEntryDialogVisible = false;
        NewEntryDialogViewModel.Name = string.Empty;
        this.RaisePropertyChanged(nameof(NewEntryDialogViewModel));
    }

    public void CloseSnackbar()
    {
        IsSnackbarVisible = false;
    }

    #endregion

    #region Private methods

    private void Snackbar_OnShow(string message, string severity = "accent", int duration = 3000)
    {
        SnackbarSeverityAccent = severity == "accent";
        SnackbarSeverityWarning = severity == "warning";
        SnackbarSeveritySuccess = severity == "success";
        SnackbarSeverityError = severity == "error";
        SnackbarText = message;
        IsSnackbarVisible = true;

        Task.Run(async () =>
        {
            await Task.Delay(duration);
            Dispatcher.UIThread.Invoke(() => { IsSnackbarVisible = false; });
        });
    }

    #endregion
}