using System.Text;
using System.Text.RegularExpressions;
using Horus.Commands.Abstractions;
using Horus.Core.Services;
using Horus.Shared;
using Horus.Shared.Enums;
using Horus.Shared.Helpers;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Errors.Fs;
using Spectre.Console;

namespace Horus.Commands;

public class Edit : ICommand
{
    #region Constants

    private static readonly Regex RegMetadataKey = new("[a-z0-9]{1}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly string[] PasswordPlaceholders = new[] { "*", "#" };
    private const int MinPasswordDisplayLength = 20;

    #endregion

    #region Members

    private readonly Random _random = new();

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
                Save(name, ref newPasswordToSave);
                break;
            }

            if (choice == Locale.Get("thePassword"))
            {
                EditPassword(name, ref newPasswordToSave, ref lastErrorMessage);
                continue;
            }

            if (choice == Locale.Get("theMetadata"))
            {
                EditMetadata(name, metadatas, ref lastErrorMessage);
                continue;
            }
        }
    }

    #endregion

    #region Private methods

    private void Save(string name, ref Password? newPasswordToSave)
    {
        ClipboardHelper.EnsureCleared();

        if (newPasswordToSave is null) return;

        var resultEditPassword = AppService.Instance.EditPassword(name, newPasswordToSave);
        if (resultEditPassword.HasError)
        {
            AnsiConsole.MarkupLine(
                $"[{Cli.GetErrorColor(resultEditPassword.Error!.Severity)}]{resultEditPassword.Error!.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]{Locale.Get("settings.saved")}[/]");
    }

    private void EditPassword(string name, ref Password? newPasswordToSave, ref string lastErrorMessage)
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

        if (choicePassword == Locale.Get("cancel")) return;
        if (choicePassword == Locale.Get("questions.generateNewPassword"))
        {
            var (generatedPassword, errorGeneratedPassword) = AppService.Instance.GenerateNewPassword();
            if (errorGeneratedPassword is not null)
            {
                AnsiConsole.MarkupLine(
                    $"[{Cli.GetErrorColor(errorGeneratedPassword.Severity)}]{errorGeneratedPassword.Message}[/]");
                return;
            }

            var length = 0;
            var alphabet = string.Empty;
            var showPassword = false;
            var placeholderPassword = GenerateRandomPasswordString(MinPasswordDisplayLength);

            while (true)
            {
                Console.Clear();
                var generatedPasswordPanel = new Panel(PadPassword(!showPassword ? placeholderPassword : generatedPassword!.ValueAsString.EscapeMarkup()))
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

                if (choicePasswordGeneration == "Back")
                {
                    newPasswordToSave = new Password(generatedPassword!.Value!);
                    generatedPassword!.Dispose();
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

                    placeholderPassword = GenerateRandomPasswordString(length);
                    continue;
                }
            }

            return;
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
                return;
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
                return;
            }

            newPasswordConfirm = null;
            GC.Collect();

            // TODO: calculate entropy and tell if the password is good or not

            newPasswordToSave = new Password(newPassword);
            newPassword = null;
            GC.Collect();
        }
    }

    private void EditMetadata(string name, MetadataCollection metadatas, ref string lastErrorMessage)
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

        if (metadataChoice == Locale.Get("cancel")) return;

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

            return;
        }

        if (!items.ContainsKey(metadataChoice))
        {
            lastErrorMessage = Locale.Get("error.metadataNotFound");
            return;
        }

        var index = items[metadataChoice];

        var metadataActionChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{Locale.Get("questions.whatToDoWithMetadata")}?")
                .AddChoices(Locale.Get("edit"), Locale.Get("delete"), Locale.Get("cancel"))
        );

        if (metadataActionChoice == Locale.Get("cancel")) return;

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

            return;
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
    }

    private string PadPassword(string value)
    {
        if (value.Length >= MinPasswordDisplayLength) return value;

        var nb = MinPasswordDisplayLength - value.Length;
        StringBuilder spaces = new();

        for (var i = 0; i < nb; ++i)
        {
            spaces.Append(' ');
        }

        return value + spaces;
    }

    private string GenerateRandomPasswordString(int length)
    {
        if (length <= 0)
        {
            length = MinPasswordDisplayLength;
        }

        StringBuilder result = new();

        for (var i = 0; i < length; ++i)
        {
            result.Append(PasswordPlaceholders[_random.Next(0, PasswordPlaceholders.Length)]);
        }

        return result.ToString();
    }

    #endregion
}