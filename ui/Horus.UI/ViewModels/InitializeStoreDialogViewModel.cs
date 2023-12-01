﻿using System;
using Horus.Core.Services;
using Horus.UI.Enums;
using Horus.UI.Services;
using ReactiveUI;

namespace Horus.UI.ViewModels;

public class InitializeStoreDialogViewModel : ViewModelBase
{
    #region Props

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private string _gpgId = string.Empty;

    public string GpgId
    {
        get => _gpgId;
        set => this.RaiseAndSetIfChanged(ref _gpgId, value);
    }

    private string _gitUrl = string.Empty;

    public string GitUrl
    {
        get => _gitUrl;
        set => this.RaiseAndSetIfChanged(ref _gitUrl, value);
    }

    #endregion

    #region Public methods

    public bool Validate()
    {
        IsLoading = true;
        var result = ValidateGpgId() && CreateStore();
        IsLoading = false;
        return result;
    }

    #endregion

    #region Private methods

    private bool CreateStore()
    {
        var result = AppService.Instance.InitializeStoreFolder(GpgId, GitUrl);
        if (!result.HasError) return true;

        SnackbarService.Instance.Show($"Failed to initialize store: {result.Error!.Message}", SnackbarSeverity.Error, 5000);
        return false;
    }

    private bool ValidateGpgId()
    {
        var (isValidGpgId, errorVerifyGpgId) = AppService.Instance.IsGpgIdValid(GpgId);
        if (errorVerifyGpgId is not null)
        {
            SnackbarService.Instance.Show("Failed to verify GPG ID", SnackbarSeverity.Error, 5000);
            return false;
        }

        if (!isValidGpgId)
        {
            SnackbarService.Instance.Show("GPG ID is not valid", SnackbarSeverity.Error, 5000);
            return false;
        }

        return true;
    }

    #endregion
}