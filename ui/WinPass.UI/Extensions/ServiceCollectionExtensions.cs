using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using WinPass.UI.Services;
using WinPass.UI.Services.Abstractions;

namespace WinPass.UI.Extensions;

public static class ServiceCollectionExtensions
{
    #region Public methods

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        return services.AddBasicServices()
            .AddSingleton<IThemeService, ThemeService>();
    }

    #endregion

    #region Private methods

    private static IServiceCollection AddBasicServices(this IServiceCollection services)
    {
        services.AddWpfBlazorWebView();
        services.AddMudServices();
        return services;
    }

    #endregion
}