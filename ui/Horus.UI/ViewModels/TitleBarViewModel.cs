namespace Horus.UI.ViewModels;

public class TitleBarViewModel : ViewModelBase
{
    #region Props

    public string Title => nameof(Horus);
    public int TitleSize => 18;
    public int LogoSize => 20;
    public int ButtonsSize => 30;
    public int SystemButtonsWidth => 38;

    #endregion
}