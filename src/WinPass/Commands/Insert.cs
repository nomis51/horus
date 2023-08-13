﻿using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Insert : ICommand
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
            AnsiConsole.MarkupLine($"[red]{Locale.Get("cli.args.passwordNameRequired")}[/]");
            return;
        }

        var name = args[0];

        if (AppService.Instance.DoStoreEntryExists(name))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("passwordAlreadyExists", new object[] { name })}[/]");
            return;
        }

        var password = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Locale.Get("enterPassword")}: ")
                .Secret(null)
        );
        if (string.IsNullOrWhiteSpace(password))
        {
            password = null;
            GC.Collect();
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.passwordEmpty")}[/]");
            return;
        }

        var passwordConfirm = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Locale.Get("confirmPassword")}: ")
                .Secret(null)
        );
        if (!password.Equals(passwordConfirm))
        {
            password = null;
            passwordConfirm = null;
            GC.Collect();
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.passwordsDontMatch")}[/]");
            return;
        }

        passwordConfirm = null;
        GC.Collect();

        var pwd = new Password(password);
        password = null;
        GC.Collect();

        var result = AppService.Instance.InsertPassword(name, pwd);

        if (result.HasError)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(result.Error!.Severity)}]{result.Error!.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine(Locale.Get("passwordCreated", new object[] { name }));
    }

    #endregion
}