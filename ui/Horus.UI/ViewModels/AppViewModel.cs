using Horus.Shared.Helpers;

namespace Horus.UI.ViewModels;

public class AppViewModel : ViewModelBase
{
    #region Props

    public string VersionText => $"Version {VersionHelper.GetVersion()}";
   
    #endregion
}