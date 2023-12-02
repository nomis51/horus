using System;
using Horus.Core.Services;
using Horus.Shared.Models.Data;
using Horus.UI.Enums;
using Horus.UI.Services;
using ReactiveUI;
using Serilog;

namespace Horus.UI.ViewModels;

public class SettingsDialogViewModel : ViewModelBase
{
    #region Props

    public Settings Settings { get; private set; } = new();

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private bool _hasChanges;

    public bool HasChanges
    {
        get => _hasChanges;
        set => this.RaiseAndSetIfChanged(ref _hasChanges, value);
    }

    #endregion

    #region Constructors

    public SettingsDialogViewModel()
    {
        RetrieveSettings();
    }

    #endregion

    #region Public methods

    public void ConfirmDestroyStore()
    {
        if (HasChanges)
        {
            SnackbarService.Instance.Show("Save settings first", SnackbarSeverity.Warning, 5000);
            return;
        }

        DialogService.Instance.Show(DialogType.DestroyStore);
    }

    public void PerformChanges()
    {
        HasChanges = true;
    }

    public bool SaveSettings()
    {
        IsLoading = true;
        var result = AppService.Instance.SaveSettings(Settings);
        IsLoading = false;

        if (result.HasError)
        {
            Log.Error("Failed to save settings: {Message}", result.Error!.Message);
            SnackbarService.Instance.Show("Failed to save settings", SnackbarSeverity.Error, 5000);
            return false;
        }

        SnackbarService.Instance.Show("Settings saved", SnackbarSeverity.Success);
        return true;
    }

    #endregion

    #region Private methods

    private void RetrieveSettings()
    {
        var (settings, error) = AppService.Instance.GetSettings();
        if (error is not null)
        {
            Log.Error("Failed to retrieve settings: {Message}", error.Message);
            SnackbarService.Instance.Show("Failed to retrieve settings", SnackbarSeverity.Error, 5000);
            return;
        }

        Settings = settings!;
    }

    #endregion
}