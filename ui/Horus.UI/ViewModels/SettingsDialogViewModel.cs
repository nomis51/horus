using System;
using Horus.Core.Services;
using Horus.Shared.Models.Data;
using Horus.UI.Enums;
using Horus.UI.Services;
using ReactiveUI;

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

    #endregion

    #region Constructors

    public SettingsDialogViewModel()
    {
        RetrieveSettings();
    }

    #endregion

    #region Public methods

    public bool SaveSettings()
    {
        IsLoading = true;
        var result = AppService.Instance.SaveSettings(Settings);
        IsLoading = false;

        if (result.HasError)
        {
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
            SnackbarService.Instance.Show("Failed to retrieve settings", SnackbarSeverity.Error, 5000);
            return;
        }

        Settings = settings!;
    }

    #endregion
}