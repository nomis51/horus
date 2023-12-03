using Horus.Core.Services;
using Horus.Enums;
using Horus.Services;
using ReactiveUI;
using Serilog;

namespace Horus.ViewModels;

public class DeleteEntryDialogViewModel : ViewModelBase
{
    #region Props

    public string Name { get; set; } = string.Empty;

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public string Message => $"Are you sure you want to delete the entry '{Name}'?";

    #endregion

    #region Public methods

    public bool DeleteEntry()
    {
        IsLoading = true;
        var result = AppService.Instance.DeleteStoreEntry(Name);
        IsLoading = false;

        if (result.HasError)
        {
            Log.Error("Failed to delete entry '{Name}': {Message}", Name, result.Error!.Message);
            SnackbarService.Instance.Show("Failed to delete entry", SnackbarSeverity.Error, 5000);
            return false;
        }

        SnackbarService.Instance.Show("Entry deleted", SnackbarSeverity.Success);
        return true;
    }

    #endregion
}