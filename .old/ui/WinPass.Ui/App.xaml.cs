using System.Windows.Threading;
using Serilog;
using Serilog.Events;

namespace WinPass.UI;

public partial class App
{
    #region Constructors

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("./logs/.txt", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    #endregion

    #region Private methods

    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal("Application fatal exception: {Message} {StackTrace}", e.Exception.Message, e.Exception.StackTrace);
    }

    #endregion
}