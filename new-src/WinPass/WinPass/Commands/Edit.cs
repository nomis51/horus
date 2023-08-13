using System.Text.RegularExpressions;
using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Enums;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Edit : ICommand
{
    #region Constants

    private static readonly Regex RegMetadataKey = new("[a-z0-9]{1}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    #endregion

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

        var name = args[^1];
        
        if (!AppService.Instance.DoStoreEntryExists(name))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.passwordDoesntExists")}[/]");
            return; 
        }

        var (metadatas, error) = AppService.Instance.GetMetadatas(name);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        var lastErrorMessage = string.Empty;
        while (true)
        {
            Console.Clear();
            if (!string.IsNullOrEmpty(lastErrorMessage))
            {
                AnsiConsole.MarkupLine($"[red]{lastErrorMessage.EscapeMarkup()}[/]");
                lastErrorMessage = string.Empty;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{Locale.Get("questions.whatToEditOn")} [blue]{name}[/]?")
                    .AddChoices(
                        Locale.Get("thePassword"),
                        Locale.Get("theMetadata"),
                        Locale.Get("quit")
                    )
            );

            if (choice == Locale.Get("quit")) break;

            if (choice == Locale.Get("thePassword"))
            {
                var choicePassword = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title(Locale.Get("questions.whatToDo"))
                        .AddChoices(
                            Locale.Get("questions.generateNewPassword"),
                            Locale.Get("questions.enterPasswordManually"),
                            Locale.Get("cancel")
                        )
                );

                if (choicePassword == Locale.Get("cancel")) continue;
                if (choicePassword == Locale.Get("questions.generateNewPassword"))
                {
                    var resultGenerateNewPassword = AppService.Instance.GenerateNewPassword(name);
                    if (resultGenerateNewPassword.HasError)
                    {
                        AnsiConsole.MarkupLine(
                            $"[{Cli.GetErrorColor(resultGenerateNewPassword.Error!.Severity)}]{resultGenerateNewPassword.Error!.Message}[/]");
                        return;
                    }

                    // TODO: copy new password to clipboard
                    continue;
                }

                if (choicePassword == Locale.Get("questions.enterPasswordManually"))
                {
                    var newPassword = AnsiConsole.Prompt(
                        new TextPrompt<string>(Locale.Get("questions.enterNewPassword"))
                            .Secret(null)
                    );
                    if (string.IsNullOrWhiteSpace(newPassword))
                    {
                        lastErrorMessage = Locale.Get("error.passwordCantEmpty");
                        continue;
                    }

                    var newPasswordConfirm = AnsiConsole.Prompt(
                        new TextPrompt<string>(Locale.Get("questions.confirmNewPassword"))
                            .Secret(null)
                    );
                    if (!newPassword.Equals(newPasswordConfirm))
                    {
                        newPassword = null;
                        newPasswordConfirm = null;
                        GC.Collect();
                        lastErrorMessage = Locale.Get("error.passwordDontMatch");
                        continue;
                    }

                    newPasswordConfirm = null;
                    GC.Collect();

                    var pwd = new Password(newPassword);
                    newPassword = null;
                    GC.Collect();

                    var resultEditPassword = AppService.Instance.EditPassword(name, pwd);
                    pwd.Dispose();

                    if (resultEditPassword.HasError)
                    {
                        AnsiConsole.MarkupLine(
                            $"[{Cli.GetErrorColor(resultEditPassword.Error!.Severity)}]{resultEditPassword.Error!.Message}[/]");
                        return;
                    }

                    continue;
                }

                continue;
            }

            if (choice == Locale.Get("theMetadata"))
            {
                Dictionary<string, int> items = new();
                for (var i = 0; i < metadatas.Count; ++i)
                {
                    if (metadatas[i].Type != MetadataType.Normal) continue;

                    items.Add($"[green]{metadatas[i].Key}[/]: {metadatas[i].Value}", i);
                }

                var metadataChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"{Locale.Get("questions.whatMetadataToEdit")}?")
                        .AddChoices(
                            items.Keys.Concat(new[] { Locale.Get("addNewMetadata"), Locale.Get("cancel") })
                        )
                );

                if (metadataChoice == Locale.Get("cancel")) continue;

                if (metadataChoice == Locale.Get("addNewMetadata"))
                {
                    var newKey = AnsiConsole.Prompt(
                        new TextPrompt<string>($"{Locale.Get("questions.enterTheKey")}: ")
                            .Validate(e =>
                            {
                                if (string.IsNullOrEmpty(e)) return false;

                                var match = RegMetadataKey.Match(e[..1]);
                                return match.Success & match.Index == 0 && match.Length == 1;
                            })
                            .ValidationErrorMessage(
                                Locale.Get("error.metadataKeyInvalid"))
                    );
                    var newValue = AnsiConsole.Ask<string>($"{Locale.Get("questions.enterTheValue")}: ");
                    metadatas.Add(new Metadata(newKey, newValue));

                    var resultEditMetadatas = AppService.Instance.EditMetadatas(name, metadatas);
                    if (resultEditMetadatas.HasError)
                    {
                        AnsiConsole.MarkupLine(
                            $"[{Cli.GetErrorColor(resultEditMetadatas.Error!.Severity)}]{resultEditMetadatas.Error!.Message}[/]");
                        return;
                    }

                    continue;
                }

                if (!items.ContainsKey(metadataChoice))
                {
                    lastErrorMessage = Locale.Get("error.metadataNotFound");
                    continue;
                }

                var index = items[metadataChoice];

                var metadataActionChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"{Locale.Get("questions.whatToDoWithMetadata")}?")
                        .AddChoices(Locale.Get("edit"), Locale.Get("delete"), Locale.Get("cancel"))
                );

                if (metadataActionChoice == Locale.Get("cancel")) continue;

                if (metadataActionChoice == Locale.Get("delete"))
                {
                    metadatas.RemoveAt(index);
                    var resultEditMetadatas = AppService.Instance.EditMetadatas(name, metadatas);
                    if (resultEditMetadatas.HasError)
                    {
                        AnsiConsole.MarkupLine(
                            $"[{Cli.GetErrorColor(resultEditMetadatas.Error!.Severity)}]{resultEditMetadatas.Error!.Message}[/]");
                        return;
                    }

                    continue;
                }

                metadatas[index].Key = AnsiConsole.Prompt(
                    new TextPrompt<string>($"{Locale.Get("questions.enterTheKey")}: ")
                        .Validate(e =>
                        {
                            if (string.IsNullOrEmpty(e)) return false;

                            var reg = new Regex("[a-z0-9]{1}", RegexOptions.IgnoreCase);
                            var match = reg.Match(e[..1]);
                            return match.Success & match.Index == 0 && match.Length == 1;
                        })
                        .ValidationErrorMessage(
                            Locale.Get("error.metadataKeyInvalid"))
                        .DefaultValue(metadatas[index].Key)
                );
                metadatas[index].Value =
                    AnsiConsole.Ask($"{Locale.Get("questions.enterTheValue")}: ", metadatas[index].Value);

                var resultEditMetadatas2 = AppService.Instance.EditMetadatas(name, metadatas);
                if (resultEditMetadatas2.HasError)
                {
                    AnsiConsole.MarkupLine(
                        $"[{Cli.GetErrorColor(resultEditMetadatas2.Error!.Severity)}]{resultEditMetadatas2.Error!.Message}[/]");
                    return;
                }

                continue;
            }
        }
    }

    #endregion
}