using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Horus.Core;
using Horus.Core.Services;
using Serilog;

namespace Horus;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
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
}