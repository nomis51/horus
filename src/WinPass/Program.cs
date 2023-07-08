using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Helpers;

namespace WinPass;

public static class Program
{
    #region Public methods

    public static void Main(string[] args)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            AnsiConsole.MarkupLine(Locale.Get("error.osNotSupported"));
            return;
        }

        Console.OutputEncoding = Encoding.UTF8;

        var dirName = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        var logDir = Path.Join(dirName, "logs");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Join(logDir, ".txt"), LogEventLevel.Information,
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var (hasUpdate, _, newVersion) = UpdateHelper.CheckForUpdate().Result;
        if (hasUpdate)
        {
            AnsiConsole.MarkupLine(
                Locale.Get("newVersionAvailable",
                    new object[] { newVersion!.ToString(), "https://github.com/nomis51/winpass/releases/latest" })
            );
        }

        UpdateHelper.EnsureAppLinked();

        try
        {
            var (settings, error) = AppService.Instance.GetSettings();
            if (error is null && settings is not null)
            {
                Locale.SetLanguage(settings.Language);
            }

            new Cli().Run(args);
        }
        catch (Exception e)
        {
            Log.Error("Unexpected error occured: {Message}{Skip}{CallStack}", e.Message, Environment.NewLine,
                e.StackTrace);
            AnsiConsole.MarkupLine(Locale.Get("error.occured", new object[] { logDir }));
        }
        finally
        {
            AppService.Instance.ReleaseLock();
        }
    }

    #endregion
}