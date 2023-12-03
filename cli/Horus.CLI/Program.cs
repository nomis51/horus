using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Horus.Core.Services;
using Horus.Shared;
using Horus.Shared.Helpers;
using Serilog;
using Serilog.Events;
using Spectre.Console;

namespace Horus;

public static class Program
{
    #region Constants

    private const string ReleasesUrl = "https://github.com/nomis51/winpass/releases/latest";

    #endregion

    #region Members

    private static string _exitMessage = string.Empty;

    #endregion

    #region Public methods

    public static void Main(string[] args)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            AnsiConsole.MarkupLine(Locale.Get("error.osNotSupported"));
            return;
        }

        Console.OutputEncoding = Encoding.UTF8;

        var logDir = InitializeLogger();
        CheckForUpdates();

        try
        {
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
            if (!string.IsNullOrEmpty(_exitMessage)) AnsiConsole.MarkupLine(_exitMessage);

            ClipboardHelper.EnsureCleared();
            AppService.Instance.ReleaseLock();
        }
    }

    #endregion

    #region Private methods

    private static string InitializeLogger()
    {
        var dirName = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        var logDir = Path.Join(dirName, "logs");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Join(logDir, ".txt"), LogEventLevel.Information,
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        return logDir;
    }

    private static void CheckForUpdates()
    {
        _ = Task.Run(() =>
        {
            var (hasUpdate, _, newVersion) = UpdateHelper.CheckForUpdate().Result;
            if (hasUpdate)
            {
                _exitMessage = Locale.Get("newVersionAvailable",
                    new object[] { newVersion!.ToString(), ReleasesUrl });
            }

            UpdateHelper.EnsureAppLinked();
        });
    }

    #endregion
}