﻿using Horus.Commands.Abstractions;
using Horus.Core.Services;
using Horus.Shared;
using Horus.Shared.Models.Errors.Fs;
using Spectre.Console;

namespace Horus.Commands;

public class Export : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        if (!AppService.Instance.IsStoreInitialized())
        {
            AnsiConsole.MarkupLine($"[red]{new FsStoreNotInitializedError().Message}[/]");
            return;
        }

        var savePath = AnsiConsole.Ask(Locale.Get("questions.exportPath"),
            Environment.ExpandEnvironmentVariables(
                Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop")));

        var result = AppService.Instance.ExportStore(savePath);
        if (result.HasError)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(result.Error!.Severity)}]{result.Error!.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]{string.Format(Locale.Get("exportDone"), savePath)}[/]");
    }

    #endregion
}