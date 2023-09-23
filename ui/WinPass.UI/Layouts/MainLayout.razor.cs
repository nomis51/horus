using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using WinPass.UI.Services.Abstractions;

namespace WinPass.UI.Layouts;

public class MainLayoutBase : LayoutComponentBase
{
    #region Services

    [Inject]
    protected IThemeService? ThemeService { get; set; }

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        ThemeService?.SetTheme(new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = new MudColor("#89b4faff"),
                BackgroundGrey = new MudColor("#151521ff"),
                Background = new MudColor("#0f0f12ff"),
                AppbarBackground = new MudColor("#1a1a27ff"),
                Surface = new MudColor("#1e1e2dff"),
                Success = new MudColor("#3dcb6cff"),
                Warning = new MudColor("#ffb545ff"),
                Error = new MudColor("#ff3f5fff"),
                Info = new MudColor("#4a86ffff"),
                Secondary = new MudColor("#ff4081ff"),
            },
        });
    }

    #endregion
}