using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;
using WinPass.UI.Services.Abstractions;

namespace WinPass.UI.Layouts;

public class MainLayoutBase : LayoutComponentBase
{
    #region Services

    [Inject]
    protected IThemeService ThemeService { get; set; }

    [Inject]
    protected IJSRuntime JsRuntime { get; set; }
    
    [Inject]
    protected ISnackbar Snackbar { get; set; }

    #endregion

    #region Members

    protected bool IsDarkMode => ThemeService.IsDarkMode;

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomLeft;
        Snackbar.Configuration.HideTransitionDuration = 300;
        Snackbar.Configuration.ShowTransitionDuration = 300;
        Snackbar.Configuration.VisibleStateDuration = 3000;
        
        ThemeService.OnThemeModeChange += OnThemeModeChange;
        ThemeService.SetTheme(new MudTheme
        {
            Palette = new PaletteDark
            {
                Primary = new MudColor("#1984b8"),
                BackgroundGrey = new MudColor("#ffffffff"),
                Background = new MudColor("#d9d9d9ff"),
                AppbarBackground = new MudColor("#ffffffff"),
                Surface = new MudColor("#f1f1f1ff"),
                Success = new MudColor("#3dcb6cff"),
                Warning = new MudColor("#ffb545ff"),
                Error = new MudColor("#ff3f5fff"),
                Info = new MudColor("#4a86ffff"),
                Secondary = new MudColor("#ff4081ff"),
                AppbarText = new MudColor("#4d4d4dff"),
                TextPrimary = new MudColor("#4d4d4dff"),
                TextSecondary = new MudColor("#5d5d5dff"),
                ActionDefault = new MudColor("#5d5d5dff"),
            },
            PaletteDark = new PaletteDark
            {
                Primary = new MudColor("#1984b8"),
                BackgroundGrey = new MudColor("#151521ff"),
                Background = new MudColor("#0e0e0eff"),
                AppbarBackground = new MudColor("#1e1e1eff"),
                Surface = new MudColor("#272626ff"),
                Success = new MudColor("#3dcb6cff"),
                Warning = new MudColor("#ffb545ff"),
                Error = new MudColor("#ff3f5fff"),
                Info = new MudColor("#4a86ffff"),
                Secondary = new MudColor("#ff4081ff"),
            },
        });
    }

    private void OnThemeModeChange(bool isDarkMode)
    {
        InvokeAsync(StateHasChanged);
    }

    #endregion
}