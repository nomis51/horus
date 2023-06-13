using System.Text.RegularExpressions;
using Spectre.Console;
using WinPass.Core.Services;
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

        switch (args[0])
        {
            case "init":
                Init(args.Skip(1).ToArray());
                break;

            case "ls":
            case "list":
                List();
                break;

            case "show":
                Show(args.Skip(1).ToArray());
                break;
        }
    }

    #endregion

    #region Commands

    private void Show(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || string.IsNullOrEmpty(args[0]))
        {
            AnsiConsole.MarkupLine("[red]password name argument required[/]");
            return;
        }

        var copy = args.Count == 2 && args[0] == "-c";
        var name = args.Count == 2 ? args[1] : args[0];

        var password = AppService.Instance.GetPassword(name);
        if (string.IsNullOrEmpty(password)) return;

        if (copy)
        {
            // TODO: to clipboard
            return;
        }

        AnsiConsole.MarkupLine("Terminal will clear in 10 seconds");
        AnsiConsole.MarkupLine($"Password for [blue]{name}[/] is [yellow]{password}[/]");

        Task.Run(() =>
        {
            Thread.Sleep(10 * 1000);
            Console.Clear();
        });
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