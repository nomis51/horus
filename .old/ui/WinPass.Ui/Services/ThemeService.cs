using System.Threading.Tasks;
using Microsoft.JSInterop;
using MudBlazor;
using WinPass.UI.Services.Abstractions;

namespace WinPass.UI.Services;

public class ThemeService : IThemeService
{
    #region Events

    public delegate void ThemeModeChangeEvent(bool isDarkMode);

    public event ThemeModeChangeEvent OnThemeModeChange;

    #endregion

    #region Props

    public bool IsDarkMode { get; private set; } = true;
    public MudTheme Theme { get; private set; }

    #endregion

    #region Public methods

    public void SetTheme(MudTheme theme)
    {
        Theme = theme;
    }

    public void SetThemeMode(IJSRuntime runtime, bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        OnThemeModeChange?.Invoke(IsDarkMode);
    }

    #endregion
}