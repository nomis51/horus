using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DynamicData;
using Horus.Core.Services;
using Horus.Enums;
using Horus.Helpers;
using Horus.Models;
using Horus.Services;
using ReactiveUI;
using Serilog;

namespace Horus.ViewModels;

public class TitleBarViewModel : ViewModelBase
{
    #region Props

    public SyncButtonViewModel SyncButtonViewModel { get; set; } = new();

    public string Title => nameof(Horus);
    public int TitleSize => 18;
    public int LogoSize => 20;
    public int ButtonsSize => 30;
    public int SystemButtonsWidth => 38;

    private bool _isUpdateIconVisible;

    public bool IsUpdateIconVisible
    {
        get => _isUpdateIconVisible;
        set
        {
            this.RaiseAndSetIfChanged(ref _isUpdateIconVisible, value);
            this.RaisePropertyChanged(nameof(UpdateMessage));
        }
    }

    public string UpdateMessage { get; private set; } = string.Empty;

    private ObservableCollection<StoreModel> _stores = new();

    public ObservableCollection<StoreModel> Stores
    {
        get => _stores;
        set => this.RaiseAndSetIfChanged(ref _stores, value);
    }

    #endregion

    #region Constructors

    public TitleBarViewModel()
    {
        RetrieveStores();
    }

    #endregion

    #region Public methods

    public void RestartGpg()
    {
        var (result, error) = AppService.Instance.RestartGpgAgent();
        if (error is not null)
        {
            Log.Warning("Failed to restart GPG agent: {Message}. {Output}", error.Message, result);
            SnackbarService.Instance.Show("Failed to restart GPG agent", SnackbarSeverity.Warning, 5000);
        }
        else
        {
            Log.Information("GPG agent restarted: {Message}", result);
            SnackbarService.Instance.Show("GPG agent restarted", SnackbarSeverity.Success);
        }
    }

    public void StopGpg()
    {
        var (result, error) = AppService.Instance.StopGpgAgent();
        if (error is not null)
        {
            SnackbarService.Instance.Show("Failed to stop GPG agent", SnackbarSeverity.Warning, 5000);
            Log.Warning("Failed to stop GPG agent: {Message}. {Output}", error.Message, result);
        }
        else
        {
            Log.Information("GPG agent stopped: {Message}", result);
            SnackbarService.Instance.Show("GPG agent stopped", SnackbarSeverity.Success);
        }
    }

    public void StartGpg()
    {
        var (result, error) = AppService.Instance.StopGpgAgent();
        if (error is not null)
        {
            SnackbarService.Instance.Show("Failed to start GPG agent", SnackbarSeverity.Warning, 5000);
            Log.Warning("Failed to start GPG agent: {Message}. {Output}", error.Message, result);
        }
        else
        {
            Log.Information("GPG agent started: {Message}", result);
            SnackbarService.Instance.Show("GPG agent started", SnackbarSeverity.Success);
        }
    }

    public void ShowUpdateIcon(string version)
    {
        UpdateMessage = $"New version {version} ready to be installed after a restart";
        IsUpdateIconVisible = true;
    }

    public void OpenGitHubPage()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start("explorer.exe", App.GitHubPageUrl);
            }
            else
            {
                Process.Start(App.GitHubPageUrl);
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to open '{Url}': {Message}", App.GitHubPageUrl, e.Message);
        }
    }

    public void OpenTerminal()
    {
        TerminalHelper.SpawnTerminal(AppService.Instance.GetStoreLocation());
    }

    #endregion

    #region Private methods

    private void RetrieveStores()
    {
        Task.Run(() =>
        {
            var (stores, error) = AppService.Instance.ListStores();
            if (error is not null || stores.Count == 0)
            {
                Log.Error("Failed to list stores: {Message}", error!.Message);
                SnackbarService.Instance.Show("Failed to list stores", SnackbarSeverity.Error, 5000);
                return;
            }

            Stores.Clear();
            Stores.AddRange(stores.Select(s => new StoreModel(s)));
            Stores.Add(new StoreModel("$separator"));
            Stores.Add(new StoreModel("$createButton"));
        });
    }

    #endregion
}