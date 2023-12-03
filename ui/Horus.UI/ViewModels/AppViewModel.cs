using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Horus.Shared.Helpers;
using Horus.UI.Enums;
using Horus.UI.Services;
using Horus.UI.Windows;
using ReactiveUI;
using UpdateHelper = Horus.UI.Helpers.UpdateHelper;

namespace Horus.UI.ViewModels;

public class AppViewModel : ViewModelBase
{
    #region Props

    private string _versionText = $"Version {VersionHelper.GetVersion()}";

    public string VersionText
    {
        get => _versionText;
        set => this.RaiseAndSetIfChanged(ref _versionText, value);
    }

    #endregion

    #region Constructors

    public AppViewModel()
    {
        CheckForUpdates();
    }

    #endregion

    #region Private methods

    private void CheckForUpdates()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(5000);
            var version = await UpdateHelper.CheckForUpdates();
            if (string.IsNullOrEmpty(version)) return;

            SnackbarService.Instance.Show($"Update {version} ready to be installed!", SnackbarSeverity.Accent, 8000);
            VersionText += $" (Version {version} will be installed after a restart)";

            if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;

            InvokeUi(() => ((MainWindow)desktop.MainWindow!).UpdateAvailable(version));
        });
    }

    #endregion
}