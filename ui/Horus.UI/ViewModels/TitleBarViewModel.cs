using System.Diagnostics;
using System.Runtime.InteropServices;
using Horus.Core.Services;
using Horus.UI.Helpers;

namespace Horus.UI.ViewModels;

public class TitleBarViewModel : ViewModelBase
{
    #region Constants

    private const string GitHubPageUrl = "https://github.com/nomis51/horus";

    #endregion

    #region Props

    public string Title => nameof(Horus);
    public int TitleSize => 18;
    public int LogoSize => 20;
    public int ButtonsSize => 30;
    public int SystemButtonsWidth => 38;

    #endregion

    #region Public methods

    public void OpenGitHubPage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start("explorer.exe", GitHubPageUrl);
        }
    }

    public void OpenSettings()
    {
        
    }

    public void OpenTerminal()
    {
        TerminalHelper.SpawnTerminal(AppService.Instance.GetStoreLocation());
    }

    #endregion
}