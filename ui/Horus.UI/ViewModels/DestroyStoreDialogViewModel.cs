using System;
using Horus.Core.Services;
using Horus.UI.Enums;
using Horus.UI.Services;
using ReactiveUI;
using Serilog;

namespace Horus.UI.ViewModels;

public class DestroyStoreDialogViewModel : ViewModelBase
{
    #region Props

    private bool _firstConfirm;

    public bool FirstConfirm
    {
        get => _firstConfirm;
        set
        {
            this.RaiseAndSetIfChanged(ref _firstConfirm, value);
            this.RaisePropertyChanged(nameof(IsFirstConfirmVisible));
            this.RaisePropertyChanged(nameof(IsSecondConfirmVisible));
        }
    }

    private bool _secondConfirm;

    public bool SecondConfirm
    {
        get => _secondConfirm;
        set
        {
            this.RaiseAndSetIfChanged(ref _secondConfirm, value);
            this.RaisePropertyChanged(nameof(IsSecondConfirmVisible));
            this.RaisePropertyChanged(nameof(IsThirdConfirmVisible));
        }
    }

    public bool ThirdConfirm => RepositoryName == _gitRepositoryName;

    private string _repositoryName = string.Empty;

    public string RepositoryName
    {
        get => _repositoryName;
        set
        {
            this.RaiseAndSetIfChanged(ref _repositoryName, value);
            this.RaisePropertyChanged(nameof(ThirdConfirm));
        }
    }

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public bool IsFirstConfirmVisible => !FirstConfirm;
    public bool IsSecondConfirmVisible => FirstConfirm && !SecondConfirm;
    public bool IsThirdConfirmVisible => FirstConfirm && SecondConfirm;

    private string _gitRepositoryName = string.Empty;

    #endregion

    #region Constructors

    public DestroyStoreDialogViewModel()
    {
        RetrieveRepositoryName();
    }

    #endregion

    #region Public methods

    public bool DestroyStore()
    {
        IsLoading = true;
        SpinnerOverlayService.Instance.Show();
        var result = AppService.Instance.DestroyStore();
        IsLoading = false;

        if (result.HasError)
        {
            Log.Error("Failed to destroy the store: {Message}", result.Error!.Message);
            SpinnerOverlayService.Instance.Hide();
            SnackbarService.Instance.Show("Failed to destroy the store", SnackbarSeverity.Error, 5000);
            return false;
        }

        SnackbarService.Instance.Show("Store destroyed", SnackbarSeverity.Success);
        return true;
    }

    public void PerformFirstConfirm()
    {
        FirstConfirm = true;
    }

    public void PerformSecondConfirm()
    {
        SecondConfirm = true;
    }

    #endregion

    #region Private methods

    private void RetrieveRepositoryName()
    {
        var (name, error) = AppService.Instance.GitGetRemoteRepositoryName();
        if (error is not null)
        {
            Log.Error("Failed to retrieve repository name: {Message}", error.Message);
            SnackbarService.Instance.Show("Failed to retrieve repository name", SnackbarSeverity.Error, 5000);
            return;
        }

        _gitRepositoryName = name;
    }

    #endregion
}