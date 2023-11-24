using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Commands;

public class Generate : ICommand
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

        var (settings, errorSettings) = AppService.Instance.GetSettings();
        if (errorSettings is not null)
        {
            AnsiConsole.MarkupLine(
                $"[yellow]{Locale.Get("error.loadingSettings", new object[] { errorSettings.Message })}[/]");
        }

        var name = args[^1];
        var length = settings?.DefaultLength ?? 0;
        var customAlphabet = settings?.DefaultCustomAlphabet ?? string.Empty;

        for (var i = 0; i < args.Count - 1; ++i)
        {
            if (args[i].StartsWith("-s="))
            {
                var value = args[i].Split("=").Last();
                if (!int.TryParse(value, out var intValue)) continue;

                length = intValue;
            }

            if (args[i].StartsWith("-a="))
            {
                var value = args[i].Split("=").Last();
                if (string.IsNullOrWhiteSpace(value)) continue;

                customAlphabet = value;
            }
        }

        var (resultGenerateNewPassword, resultGenerateNewPasswordError) = AppService.Instance.GenerateNewPassword(length, customAlphabet);

        if (resultGenerateNewPasswordError is not null)
        {
            AnsiConsole.MarkupLine(
                $"[{Cli.GetErrorColor(resultGenerateNewPasswordError.Severity)}]{resultGenerateNewPasswordError.Message}[/]");
            return;
        }

        var insertPasswordResult = AppService.Instance.InsertPassword(name, resultGenerateNewPassword!);

        if (insertPasswordResult.HasError)
        {
            AnsiConsole.MarkupLine(
                $"[{Cli.GetErrorColor(insertPasswordResult.Error!.Severity)}]{insertPasswordResult.Error!.Message}[/]");
            return;
        }
        
        AnsiConsole.MarkupLine(Locale.Get("passwordGenerated", new object[] { name }));
    }

    #endregion
}