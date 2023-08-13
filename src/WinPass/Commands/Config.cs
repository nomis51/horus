using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Config : ICommand
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

        var (settings, error) = AppService.Instance.GetSettings();
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (settings is null) return;

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(Locale.Get("questions.whatToEdit") + "?")
                    .AddChoices(
                        Locale.Get("settings.defaultPasswordLength"),
                        Locale.Get("settings.defaultCustomAlphabet"),
                        Locale.Get("settings.defaultClearTimeout"),
                        Locale.Get("settings.language"),
                        Locale.Get("save"),
                        Locale.Get("cancel")
                    )
            );

            if (choice == Locale.Get("cancel")) return;

            if (choice == Locale.Get("save"))
            {
                var result = AppService.Instance.SaveSettings(settings);
                if (result.HasError)
                {
                    AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(result.Error!.Severity)}]{result.Error!.Message}[/]");
                    return;
                }

                AnsiConsole.MarkupLine($"[green]{Locale.Get("settings.saved")}[/]");
                return;
            }

            if (choice == Locale.Get("settings.defaultPasswordLength"))
            {
                settings.DefaultLength =
                    AnsiConsole.Ask($"{Locale.Get("questions.passwordLength")}: ", settings.DefaultLength);
            }

            if (choice == Locale.Get("settings.defaultCustomAlphabet"))
            {
                var value = AnsiConsole.Ask($"{Locale.Get("questions.customAlphabet")}: ",
                    settings.DefaultCustomAlphabet);
                settings.DefaultCustomAlphabet = value == "r" ? string.Empty : value;
            }

            if (choice == Locale.Get("settings.defaultClearTimeout"))
            {
                settings.ClearTimeout =
                    AnsiConsole.Ask($"{Locale.Get("questions.clearTimeout")}: ", settings.ClearTimeout);
            }

            if (choice == Locale.Get("settings.language"))
            {
                settings.Language = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title(Locale.Get("questions.language"))
                            .AddChoices(
                                nameof(Locale.English),
                                nameof(Locale.French),
                                nameof(Locale.German)
                                // TODO: add more
                            )
                    ) switch
                    {
                        "French" => Locale.French,
                        "German" => Locale.German,
                        _ => Locale.English
                    };
            }
        }
    }

    #endregion
}