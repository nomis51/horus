using MudBlazor;
using WinPass.UI.Services.Abstractions;

namespace WinPass.UI.Services;

public class ThemeService : IThemeService
{
    #region Props

    public MudTheme Theme { get; private set; } = null!;

    #endregion

    #region Public methods

    public void SetTheme(MudTheme theme)
    {
        Theme = theme;
    }

    #endregion
}