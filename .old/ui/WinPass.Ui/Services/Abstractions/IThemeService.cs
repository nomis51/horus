using Microsoft.JSInterop;
using MudBlazor;

namespace WinPass.UI.Services.Abstractions;

public interface IThemeService
{
    public event ThemeService.ThemeModeChangeEvent OnThemeModeChange;
    MudTheme Theme { get; }
    bool IsDarkMode { get; }
    void SetTheme(MudTheme theme);
    void SetThemeMode(IJSRuntime runtime, bool isDarkMode);
}