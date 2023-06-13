using Spectre.Console;

namespace WinPass;

public static class Program
{
    #region Public methods

    public static void Main(string[] args)
    {
        new Cli().Run(args);
        
        // AnsiConsole.WriteLine("Enter arguments: ");
        // var input = Console.ReadLine();
        // if (string.IsNullOrEmpty(input)) return;
        //
        // new Cli().Run(input.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));        
    }

    #endregion
}