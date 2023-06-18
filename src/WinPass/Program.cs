using System.Reflection;
using Serilog;
using Serilog.Events;

namespace WinPass;

public static class Program
{
    #region Public methods

    public static void Main(string[] args)
    {
        var dirName = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Join(dirName, "..", "logs", ".txt"), LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        if (!args.Contains("update"))
        {
            Updater.Verify().Wait();
        }

        try
        {
            new Cli().Run(args);
        }
        catch (Exception e)
        {
            Log.Error("Unexpected error occured: {Message}{Skip}{CallStack}", e.Message, Environment.NewLine,
                e.StackTrace);
        }
    }

    #endregion
}