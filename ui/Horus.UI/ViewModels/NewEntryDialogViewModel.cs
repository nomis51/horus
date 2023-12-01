using System.Threading.Tasks;
using Horus.Core.Services;
using Horus.Shared.Models.Data;
using Horus.UI.Enums;
using Horus.UI.Services;
using ReactiveUI;

namespace Horus.UI.ViewModels;

public class NewEntryDialogViewModel : ViewModelBase
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

    public bool CreateEntry()
    {
        IsLoading = true;
        var result = AppService.Instance.InsertPassword(Name, new Password("*****"));

        IsLoading = false;
        if (!result.HasError) return true;

        SnackbarService.Instance.Show("Failed to create the entry", SnackbarSeverity.Warning);
        return false;
    }

    #endregion
}