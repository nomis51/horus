namespace WinPass;

public static class Program
{
    #region Public methods

    public static void Main(string[] args)
    {
        if (!args.Contains("update"))
        {
            Updater.Verify().Wait();
        }

        new Cli().Run(args);
    }

    #endregion
}