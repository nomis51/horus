using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Horus.Core;
using Horus.Core.Services;
using Horus.Helpers;
using Serilog;

namespace Horus;

sealed class Program
{
    #region Public methods

    [STAThread]
    public static void Main(string[] args)
    {
        HandleCommands(args);
        UpdateHelper.HookSquirrel();
        InitializeServices();

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Error("Unhandled exception: {Message} {StackTrace}", e.Message, e.StackTrace);
        }
    }

    #endregion

    #region Private methods

    private static void InitializeServices()
    {
        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new FsService(),
            new GitService(),
            new GpgService(),
            new SettingsService()
        ));
        AppService.Instance.AcquireLock();
    }

    private static void HandleCommands(string[] args)
    {
        if (args is not ["cc", _] || !int.TryParse(args[1], out var delay)) return;

        ClipboardHelper.SafeClear(delay);
        Environment.Exit(0);
    }

    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
    }

    #endregion
}