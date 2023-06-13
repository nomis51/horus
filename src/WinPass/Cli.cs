using System.Text.RegularExpressions;
using Spectre.Console;
using WinPass.Core.Services;
using WinPass.Core.WinApi;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models.Fs;

namespace WinPass;

public class Cli
{
    #region Constants

    private static readonly Regex RegGpgKeyId =
        new("[a-z0-9]{40}|[a-z0-9]{16}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion

    #region Public methods

    public void Run(string[] args)
    {
        if (args.Length == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No arguments provided[/]");
            return;
        }

        var commandArgs = args.Skip(1).ToArray();
        switch (args[0])
        {
            case "init":
                Init(commandArgs);
                break;

            case "ls":
            case "list":
                List();
                break;

            case "show":
                Show(commandArgs);
                break;
            
            case "insert":
                case "add":

                break;

            case "cc":
                ClearClipboard(commandArgs);
                break;
        }
    }

    #endregion

    #region Commands

    private void Insert(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]password name argument required[/]");
            return;
        }

        var name = args.First();
        
        
    }
    
    private void ClearClipboard(IReadOnlyList<string> args)
    {
        if (args.Count == 0) return;
        if (!int.TryParse(args[0], out var intValue)) return;

        Thread.Sleep(1000 * intValue);
        User32.ClearClipboard();
    }

    private void Show(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || string.IsNullOrEmpty(args[0]))
        {
            AnsiConsole.MarkupLine("[red]password name argument required[/]");
            return;
        }

        var copy = args.Count == 2 && args[0] == "-c";
        var name = args.Count == 2 ? args[1] : args[0];

        var password = AppService.Instance.GetPassword(name, copy);
        if (copy)
        {
            AnsiConsole.MarkupLine("Password copied");
            AnsiConsole.MarkupLine("[yellow]Clipboard will be cleared in 10 seconds[/]");
            return;
        }

        if (password is null) return;

        Table table = new();
        table.AddColumn("Key");
        table.AddColumn("Value");

        foreach (var metadata in password.Metadata)
        {
            table.AddRow(metadata.Key, metadata.Value);
        }

        AnsiConsole.Write(table);

        AnsiConsole.MarkupLine("Terminal will clear in 10 seconds");
        AnsiConsole.MarkupLine($"Password for [blue]{name}[/] is{Environment.NewLine}[yellow]{password.Value}[/]");

        var (_, top) = Console.GetCursorPosition();

        Thread.Sleep(10 * 1000);
        Console.SetCursorPosition(0, top - 3);
        for (var i = 0; i < 3; ++i)
        {
            for (var k = 0; k < Console.WindowWidth; ++k)
            {
                Console.Write(" ");
            }
        }
    }

    private void List()
    {
        var entries = AppService.Instance.ListStoreEntries()
            .ToList();
        if (!entries.Any())
        {
            AnsiConsole.MarkupLine("Store is empty");
            return;
        }

        var tree = new Tree(string.Empty);
        RenderEntries(entries, tree);
        AnsiConsole.Write(tree);
    }

    private void Init(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || string.IsNullOrEmpty(args[0]))
        {
            AnsiConsole.MarkupLine("[red]GPG key ID argument required[/]");
            return;
        }

        var match = RegGpgKeyId.Match(args[0]);
        if (!match.Success || match.Index != 0 || match.Length != args[0].Length)
        {
            AnsiConsole.MarkupLine("[red]Invalid GPG key ID provided[/]");
            return;
        }

        AppService.Instance.InitializeStoreFolder(args[0]);
    }

    #endregion

    #region Private methods

    private void RenderEntries(List<StoreEntry> entries, IHasTreeNodes node)
    {
        foreach (var entry in entries)
        {
            var n = node.AddNode(entry.Name);
            if (!entry.Entries.Any()) continue;

            RenderEntries(entry.Entries, n);
        }
    }

    #endregion
}