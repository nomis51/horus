using Horus.Enums;
using ReactiveUI;

namespace Horus.ViewModels;

public class DialogManagerViewModel : ViewModelBase
{
    #region Props

    private bool _isVisible;

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    private DialogType _dialogType = DialogType.None;

    public DialogType DialogType
    {
        get => _dialogType;
        private set
        {
            this.RaiseAndSetIfChanged(ref _dialogType, value);
            this.RaisePropertyChanged(nameof(IsNewEntryDialogVisible));
            this.RaisePropertyChanged(nameof(IsDeleteEntryDialogVisible));
            this.RaisePropertyChanged(nameof(IsDuplicateEntryDialogVisible));
            this.RaisePropertyChanged(nameof(IsInitializeStoreDialogVisible));
            this.RaisePropertyChanged(nameof(IsSettingsDialogVisible));
            this.RaisePropertyChanged(nameof(IsDestroyStoreDialogVisible));
        }
    }

    public NewEntryDialogViewModel NewEntryDialogViewModel { get; set; } = new();
    public SettingsDialogViewModel SettingsDialogViewModel { get; set; } = new();
    public DeleteEntryDialogViewModel DeleteEntryDialogViewModel { get; set; } = new();
    public DuplicateEntryDialogViewModel DuplicateEntryDialogViewModel { get; set; } = new();
    public InitializeStoreDialogViewModel InitializeStoreDialogViewModel { get; set; } = new();
    public DestroyStoreDialogViewModel DestroyStoreDialogViewModel { get; set; } = new();

    public bool IsNewEntryDialogVisible => DialogType == DialogType.NewEntry;
    public bool IsDeleteEntryDialogVisible => DialogType == DialogType.DeleteEntry;
    public bool IsDuplicateEntryDialogVisible => DialogType == DialogType.DuplicateEntry;
    public bool IsInitializeStoreDialogVisible => DialogType == DialogType.InitializeStore;
    public bool IsSettingsDialogVisible => DialogType == DialogType.Settings;
    public bool IsDestroyStoreDialogVisible => DialogType == DialogType.DestroyStore;

    #endregion

    #region Public methods

    public void ShowDialog(DialogType dialogType, object? data = null)
    {
        if (dialogType == DialogType.None) return;

        DialogType = dialogType;
        SetData(data);
        IsVisible = true;
    }

    public void CloseDialog()
    {
        IsVisible = false;
        DialogType = DialogType.None;
    }

    #endregion

    #region Private methods

    private void SetData(object? data = null)
    {
        if (data is null) return;

        switch (DialogType)
        {
            case DialogType.NewEntry:
                NewEntryDialogViewModel = new NewEntryDialogViewModel
                {
                    Name = (string)data
                };
                this.RaisePropertyChanged(nameof(NewEntryDialogViewModel));
                break;

            case DialogType.DeleteEntry:
                DeleteEntryDialogViewModel = new DeleteEntryDialogViewModel
                {
                    Name = (string)data
                };
                this.RaisePropertyChanged(nameof(DeleteEntryDialogViewModel));
                break;

            case DialogType.DuplicateEntry:
                DuplicateEntryDialogViewModel = new DuplicateEntryDialogViewModel
                {
                    Name = (string)data
                };
                this.RaisePropertyChanged(nameof(DuplicateEntryDialogViewModel));
                break;

            case DialogType.InitializeStore:
                InitializeStoreDialogViewModel = new InitializeStoreDialogViewModel
                {
                };
                this.RaisePropertyChanged(nameof(InitializeStoreDialogViewModel));
                break;

            case DialogType.Settings:
                SettingsDialogViewModel = new SettingsDialogViewModel
                {
                };
                this.RaisePropertyChanged(nameof(SettingsDialogViewModel));
                break;
            
            case DialogType.DestroyStore:
                DestroyStoreDialogViewModel = new DestroyStoreDialogViewModel
                {
                };
                this.RaisePropertyChanged(nameof(DestroyStoreDialogViewModel));
                break;
        }
    }

    #endregion
}