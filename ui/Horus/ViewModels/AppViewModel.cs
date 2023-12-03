using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Horus.Enums;
using Horus.Services;
using Horus.Shared.Helpers;
using Horus.Windows;
using ReactiveUI;
using Serilog;
using UpdateHelper = Horus.Helpers.UpdateHelper;

namespace Horus.ViewModels;

public class AppViewModel : ViewModelBase
{
    #region Props

    private string _versionText = $"Version {VersionHelper.GetVersion()}";

    public string VersionText
    {
        get => _versionText;
        set => this.RaiseAndSetIfChanged(ref _versionText, value);
    }

    private bool _checkingForUpdates;

    #endregion

    #region Constructors

    public AppViewModel()
    {
        CheckForUpdates();
    }

    #endregion

    #region Public methods

    public void CheckForUpdates()
    {
        if (_checkingForUpdates) return;

        _checkingForUpdates = true;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(5000);
                var version = await UpdateHelper.CheckForUpdates();
                if (string.IsNullOrEmpty(version)) return;

                SnackbarService.Instance.Show($"Update {version} ready to be installed!", SnackbarSeverity.Accent, 8000);
                VersionText += $" (Version {version} will be installed after a restart)";

                if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

                InvokeUi(() => ((MainWindow)desktop.MainWindow!).UpdateAvailable(version));
            }
            catch (Exception e)
            {
                Log.Error("Failed to check for updates: {Message}", e.Message);
            }
            finally
            {
                _checkingForUpdates = false;
            }
        });
    }

    #endregion
}