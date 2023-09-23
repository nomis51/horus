using MudBlazor;

namespace WinPass.UI.Services.Abstractions;

public interface IThemeService
{
    public MudTheme Theme { get; }
    public void SetTheme(MudTheme theme);
}