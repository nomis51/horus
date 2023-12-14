using Horus.Core.Services;
using Horus.Enums;
using Horus.Services;
using Horus.Shared.Models.Data;
using ReactiveUI;

namespace Horus.ViewModels;

public class CreateStoreDialogViewModel : ViewModelBase
{
    #region Public  methods

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.RaisePropertyChanged(nameof(IsNameValid));
        }
    }

    public bool IsNameValid => !string.IsNullOrWhiteSpace(Name);

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    #endregion

    #region Public methods

    public bool CreateStore()
    {
        IsLoading = true;
        var result = AppService.Instance.CreateStore(Name);

        IsLoading = false;
        if (!result.HasError)
        {
            SnackbarService.Instance.Show($"Store '{Name}' created", SnackbarSeverity.Success);
            return true;
        }

        SnackbarService.Instance.Show("Failed to create the store", SnackbarSeverity.Warning);
        return false;
    }

    #endregion
}