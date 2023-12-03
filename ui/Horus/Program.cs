using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.ReactiveUI;
using Horus.Core;
using Horus.Core.Services;
using Serilog;
using Squirrel;

namespace Horus;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SquirrelAwareApp.HandleEvents(
                onInitialInstall: OnAppInstall,
                onAppUninstall: OnAppUninstall,
                onEveryRun: OnAppRun);
        }

        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new FsService(".horus-tests"),
            new GitService(),
            new GpgService(),
            new SettingsService()
        ));
        AppService.Instance.AcquireLock();

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

    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    private static void OnAppInstall(SemanticVersion version, IAppTools tools)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }
    }

    private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }
    }

    private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
    {
        tools.SetProcessAppUserModelId();
    }
}