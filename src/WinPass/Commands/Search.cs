﻿using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Models.Display;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Search : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        if (!Cli.AcquireLock()) return;
        if (!AppService.Instance.IsStoreInitialized())
        {
            AnsiConsole.MarkupLine($"[red]{new FsStoreNotInitializedError().Message}[/]");
            return;
        }

        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("cli.args.searchTermRequired")}[/]");
            return;
        }

        var searchMetadatas = args.Contains("-m");

        var text = args[0];

        if (!AppService.Instance.VerifyLock())
        {
            AnsiConsole.MarkupLine("[red]Unable to verify lock.[/]");
            return;
        }

        AnsiConsole.MarkupLine($"Searching{(searchMetadatas ? " (May take some time)" : string.Empty)}...");
        var (entries, error) = AppService.Instance.SearchStoreEntries(text, searchMetadatas);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (!entries.Any()) return;

        var tree = new Tree(string.Empty);
        List.RenderEntries(entries, tree);
        AnsiConsole.Write(tree);
    }

    #endregion
}