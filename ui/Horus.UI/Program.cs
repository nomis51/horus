using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Horus.Core;
using Horus.Core.Services;

namespace Horus.UI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new FsService(".winpass-tests"),
            new GitService(),
            new GpgService(),
            new SettingsService()
        ));
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}