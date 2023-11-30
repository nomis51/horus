using Horus.Core.Services;
using Horus.UI.Services;
using ReactiveUI;

namespace Horus.UI.ViewModels;

public class DuplicateEntryDialogViewModel : ViewModelBase
{
    #region Public  methods

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.RaisePropertyChanged(nameof(Title));
        }
    }

    private string _newName = string.Empty;

    public string NewName
    {
        get => _newName;
        set
        {
            this.RaiseAndSetIfChanged(ref _newName, value);
            this.RaisePropertyChanged(nameof(IsNameValid));
        }
    }

    public bool IsNameValid => !string.IsNullOrWhiteSpace(NewName);

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string Title => $"Duplicate the entry '{Name}'";

    #endregion

    #region Public methods

    public bool DuplicateEntry()
    {
        IsLoading = true;
        var result = AppService.Instance.RenameStoreEntry(Name, NewName, true);

        IsLoading = false;
        if (!result.HasError)
        {
            SnackbarService.Instance.Show("Entry created", "success");
            return true;
        }

        SnackbarService.Instance.Show("Failed to duplicate the entry", "warning");
        return false;
    }

    #endregion
}