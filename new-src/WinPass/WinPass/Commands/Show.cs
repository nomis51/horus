using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Enums;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Display;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Show : ICommand
{
    #region Public methods

    public void Run(List<string> args)
    {
        if (!AppService.Instance.IsStoreInitialized())
        {
            AnsiConsole.MarkupLine($"[red]{new FsStoreNotInitializedError().Message}[/]");
            return;
        }

        if (args.Count == 0 || string.IsNullOrEmpty(args[0]))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("cli.args.passwordNameRequired")}[/]");
            return;
        }

        var name = args[^1];
        args.RemoveAt(args.Count - 1);

        var (settings, errorSettings) = AppService.Instance.GetSettings();
        if (errorSettings is not null)
        {
            AnsiConsole.MarkupLine(
                $"[yellow]{Locale.Get("error.loadingSettings", new object[] { errorSettings.Message })}[/]");
        }

        var copy = false;
        var dontClear = false;
        var showMetadata = false;
        var showPassword = false;
        var timeout = settings?.ClearTimeout ?? 10;
        if (timeout <= 0)
        {
            timeout = 10;
        }

        foreach (var arg in args)
        {
            switch (arg)
            {
                case "-c":
                    copy = true;
                    showPassword = true;
                    break;

                case "-f":
                    dontClear = true;
                    break;

                case "-m":
                    showMetadata = true;
                    break;

                case "-p":
                    showPassword = true;
                    break;

                default:
                    if (arg.StartsWith("-t="))
                    {
                        if (!int.TryParse(arg.Split("-t=").Last(), out var intValue)) continue;
                        timeout = intValue <= 0 ? 10 : intValue;
                    }

                    break;
            }
        }

        if (!showPassword && !showMetadata)
        {
            showMetadata = true;
        }

        if (copy && dontClear)
        {
            AnsiConsole.MarkupLine($"[yellow]{Locale.Get("argfNoEfectWithArgc")}[/]");
        }

        if (showMetadata)
        {
            var (metadatas, error) = AppService.Instance.GetMetadatas(name);
            if (error is not null)
            {
                AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(error.Severity)}]{error.Message}[/]");
                return;
            }

            DisplayMetadatas(metadatas);
        }

        if (showPassword)
        {
            var (password, error) = AppService.Instance.GetPassword(name, copy, timeout);
            if (error is not null)
            {
                AnsiConsole.MarkupLine($"[{Cli.GetErrorColor(error.Severity)}]{error.Message}[/]");
                return;
            }

            if (copy)
            {
                AnsiConsole.MarkupLine($"[green]{Locale.Get("passwordCopied")}[/]");
                AnsiConsole.MarkupLine(
                    $"[yellow]{Locale.Get("clipboardWillCleared", new object[] { timeout.ToString() })}[/]");
                return;
            }

            DisplayPassword(password!);
            password!.Dispose();

            if (dontClear) return;

            AnsiConsole.MarkupLine(
                $"[yellow]{Locale.Get("terminalWillCleared", new object[] { timeout.ToString() })}[/]");

            Thread.Sleep(timeout * 1000);
            Console.Clear();
        }
    }

    #endregion

    #region Private methods

    private void DisplayMetadatas(MetadataCollection metadatas)
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
        };
        table.AddColumn(Locale.Get("key"));
        table.AddColumn(Locale.Get("value"));

        foreach (var m in metadatas.OrderBy(m => m.Type))
        {
            table.AddRow(
                m.Type switch
                {
                    MetadataType.Internal => $"[red]{Locale.Get($"metadata.{m.Key}")}[/]",
                    _ => m.Key
                },
                m.Value.Trim()
            );
        }

        AnsiConsole.Write(table);
    }

    private void DisplayPassword(Password password)
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            ShowHeaders = false,
        };

        table.AddColumn(string.Empty);
        table.AddRow($"{Locale.Get("passwordIs")} [yellow]{password.ValueAsString.EscapeMarkup()}[/]");
        AnsiConsole.Write(table);
        table.Rows.Clear();
    }

    #endregion
}