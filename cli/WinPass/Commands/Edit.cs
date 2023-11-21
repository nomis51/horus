using System.Text.RegularExpressions;
using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Enums;
using WinPass.Shared.Helpers;
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
        Password? newPasswordToSave = null;
        while (true)
        {
            Console.Clear();
            if (!string.IsNullOrEmpty(lastErrorMessage))
            {
                AnsiConsole.MarkupLine($"{lastErrorMessage.EscapeMarkup()}");
                lastErrorMessage = string.Empty;
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"{Locale.Get("questions.whatToEditOn")} [blue]{name}[/]?")
                    .AddChoices(
                        Locale.Get("thePassword"),
                        Locale.Get("theMetadata"),
                        Locale.Get("save"),
                        Locale.Get("quit")
                    )
            );

            if (choice == Locale.Get("quit")) break;

            if (choice == Locale.Get("save"))
            {
                ClipboardHelper.EnsureCleared();

                if (newPasswordToSave is not null)
                {
                    var resultEditPassword = AppService.Instance.EditPassword(name, newPasswordToSave);
                    if (resultEditPassword.HasError)
                    {
                        AnsiConsole.MarkupLine(
                            $"[{Cli.GetErrorColor(resultEditPassword.Error!.Severity)}]{resultEditPassword.Error!.Message}[/]");
                        return;
                    }

                    AnsiConsole.MarkupLine($"[green]{Locale.Get("settings.saved")}[/]");
                }

                break;
            }

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
                    var (generatedPassword, errorGeneratedPassword) = AppService.Instance.GenerateNewPassword();
                    if (errorGeneratedPassword is not null)
                    {
                        AnsiConsole.MarkupLine(
                            $"[{Cli.GetErrorColor(errorGeneratedPassword.Severity)}]{errorGeneratedPassword.Message}[/]");
                        continue;
                    }

                    var length = 0;
                    var alphabet = string.Empty;
                    var showPassword = false;

                    while (true)
                    {
                        Console.Clear();
                        var generatedPasswordPanel = new Panel(!showPassword ? "****************************" : generatedPassword!.ValueAsString.EscapeMarkup())
                            .Header("Generated password")
                            .RoundedBorder()
                            .BorderColor(Color.Blue);
                        AnsiConsole.Write(generatedPasswordPanel);

                        var choicePasswordGeneration = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .AddChoices(
                                    "Generate a new one",
                                    "Show/hide the generated password",
                                    "Copy old password",
                                    "Copy new password",
                                    "Edit length",
                                    "Edit alphabet",
                                    "Back"
                                )
                        );

                        if (choicePasswordGeneration == Locale.Get("cancel")) break;
                        if (choicePasswordGeneration == "Confirm")
                        {
                            newPasswordToSave = generatedPassword;
                            generatedPassword.Dispose();
                            generatedPassword = null;
                            break;
                        }

                        if (choicePasswordGeneration == "Show/hide the generated password")
                        {
                            showPassword = !showPassword;
                            continue;
                        }

                        if (choicePasswordGeneration == "Copy old password")
                        {
                            var (_, existingPasswordError) = AppService.Instance.GetPassword(name);
                            if (existingPasswordError is not null)
                            {
                                AnsiConsole.MarkupLine(
                                    $"[{Cli.GetErrorColor(existingPasswordError.Severity)}]{existingPasswordError.Message}[/]");
                            }

                            continue;
                        }

                        if (choicePasswordGeneration == "Copy new password")
                        {
                            ClipboardHelper.Copy(generatedPassword!.ValueAsString);
                            ProcessHelper.Fork(new[] { "cc", "10" });
                            continue;
                        }

                        if (choicePasswordGeneration == "Edit length")
                        {
                            length = AnsiConsole.Ask("Length", 18);
                            choicePasswordGeneration = "Generate a new one";
                        }

                        if (choicePasswordGeneration == "Edit alphabet")
                        {
                            alphabet = AnsiConsole.Ask("Alphabet", string.Empty);
                            choicePasswordGeneration = "Generate a new one";
                        }

                        if (choicePasswordGeneration == "Generate a new one")
                        {
                            var (g, e) = AppService.Instance.GenerateNewPassword(length, alphabet);
                            if (e is not null)
                            {
                                AnsiConsole.MarkupLine(
                                    $"[{Cli.GetErrorColor(e.Severity)}]{e.Message}[/]");
                                continue;
                            }

                            generatedPassword = new Password(g!.Value!);
                            g.Dispose();
                            g = null;
                            continue;
                        }
                    }

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
                        lastErrorMessage = $"[red]{Locale.Get("error.passwordCantEmpty")}[/]";
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
                        lastErrorMessage = $"[red]{Locale.Get("error.passwordDontMatch")}[/]";
                        continue;
                    }

                    newPasswordConfirm = null;
                    GC.Collect();

                    // TODO: calculate entropy and tell if the password is good or not

                    newPasswordToSave = new Password(newPassword);
                    newPassword = null;
                    GC.Collect();

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