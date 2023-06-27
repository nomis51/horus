using System.Text.RegularExpressions;
using Spectre.Console;
using WinPass.Core.Services;
using WinPass.Shared;
using WinPass.Shared.Enums;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Fs;

namespace WinPass;

public class Cli
{
    #region Constants

    private static readonly Regex RegGpgKeyId =
        new("[a-z0-9]{40}|[a-z0-9]{16}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex RegMetadataKey = new("[a-z0-9]{1}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    #endregion

    #region Public methods

    public void Run(string[] args)
    {
        var (_, error) = AppService.Instance.Verify();
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[red]{error.Message}[/]");
            return;
        }

        if (args.Length == 0)
        {
            args = new[] { "ls" };
        }

        var commandArgs = args.Skip(1).ToList();
        switch (args[0])
        {
            case "init":
                Init();
                break;

            case "ls":
            case "list":
                List();
                break;

            case "show":
                Show(commandArgs);
                break;

            case "insert":
            case "add":
                Insert(commandArgs);
                break;

            case "edit":
                Edit(commandArgs);
                break;

            case "grep":
            case "find":
            case "search":
                Search(commandArgs);
                break;

            case "generate":
                Generate(commandArgs);
                break;

            case "delete":
            case "remove":
                Delete(commandArgs);
                break;

            case "rename":
            case "move":
                Rename(commandArgs);
                break;

            case "help":
                Help();
                break;

            case "version":
                ShowVersion();
                break;

            case "git":
                Git(commandArgs);
                break;

            case "cc":
                ClearClipboard(commandArgs);
                break;

            case "config":
                Config();
                break;

            case "destroy":
                Destroy();
                break;

            default:
                AnsiConsole.MarkupLine("[red]Invalid command[/]");
                break;
        }

        AppService.Instance.ReleaseLock();
    }

    #endregion

    #region Commands

    private void Destroy()
    {
        if (!AcquireLock()) return;

        var (isAhead, isAheadError) = AppService.Instance.IsAheadOfRemote();
        if (isAheadError is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(isAheadError.Severity)}]{isAheadError.Message}[/]");
            return;
        }

        var choiceSync = Locale.Get("y");
        if (isAhead)
        {
            AnsiConsole.MarkupLine($"[yellow]{Locale.Get("repositoryAhead")}[/]");
            choiceSync =
                AnsiConsole.Ask(Locale.Get("questions.syncChangesBeforeDelete"), Locale.Get("y"));
            if (choiceSync == Locale.Get("y"))
            {
                var (_, pushError) = AppService.Instance.GitPush();
                if (pushError is not null)
                {
                    AnsiConsole.MarkupLine($"[{GetErrorColor(pushError.Severity)}]{pushError.Message}[/]");
                    return;
                }
            }
        }

        var choiceConfirmDelete =
            AnsiConsole.Ask($"{Locale.Get("questions.confirmDestroyStore")}?",
                Locale.Get("n"));
        if (choiceConfirmDelete != Locale.Get("y")) return;

        if (isAhead && choiceSync != Locale.Get("y"))
        {
            var (repositoryName, repositoryNameError) = AppService.Instance.GetRemoteRepositoryName();
            if (repositoryNameError is not null)
            {
                AnsiConsole.MarkupLine(
                    $"[{GetErrorColor(repositoryNameError.Severity)}]{repositoryNameError.Message}[/]");
                return;
            }

            AnsiConsole.MarkupLine(
                "Are you [yellow]really[/] sure you want to [red]delete[/] the store and [yellow]NOT[/] push local changes to the remote repository?");
            _ = AnsiConsole.Prompt(
                new TextPrompt<string>(
                        $"If yes, please confirm by typing the name of the remote repository [green]({repositoryName})[/]: ")
                    .Validate(v => v == repositoryName)
                    .ValidationErrorMessage("Repository name doesn't match")
            );
        }

        var (_, error) = AppService.Instance.DestroyStore();
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]{Locale.Get("storeDestroyed")}[/]");
    }

    private void Config()
    {
        if (!AcquireLock()) return;
        if (!AppService.Instance.IsStoreInitialized())
        {
            AnsiConsole.MarkupLine($"[red]{new FsStoreNotInitializedError().Message}[/]");
            return;
        }

        var (settings, error) = AppService.Instance.GetSettings();
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
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
                var (_, errorSave) = AppService.Instance.SaveSettings(settings);
                if (errorSave is not null)
                {
                    AnsiConsole.MarkupLine($"[{GetErrorColor(errorSave.Severity)}]{errorSave.Message}[/]");
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

    private void ShowVersion()
    {
        var version = VersionHelper.GetVersion();
        AnsiConsole.MarkupLine($"{Locale.Get("version")} {version.Major}.{version.Minor}.{version.Build}");
    }

    private void Git(IEnumerable<string> args)
    {
        if (!AcquireLock()) return;
        if (!AppService.Instance.IsStoreInitialized())
        {
            AnsiConsole.MarkupLine($"[red]{new FsStoreNotInitializedError().Message}[/]");
            return;
        }

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Running git command...", _ =>
            {
                var (result, error) = AppService.Instance.ExecuteGitCommand(args.ToArray());
                AnsiConsole.WriteLine(result);
                AnsiConsole.WriteLine(error);
            });
    }

    private void Help()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded
        };
        table.AddColumn(Locale.Get("help.command"));
        table.AddColumn(Locale.Get("help.description"));
        table.AddColumn(Locale.Get("help.example"));

        table.AddRow(
            "winpass init".EscapeMarkup(),
            Locale.Get("help.description.init"),
            "winpass init"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (list|ls|*blank*)".EscapeMarkup(),
            Locale.Get("help.description.ls"),
            "winpass list"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass show [args] [name]".EscapeMarkup(),
            Locale.Get("help.description.show"),
            "winpass show -c -m github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (insert|add) [name]".EscapeMarkup(),
            Locale.Get("help.description.insert"),
            "winpass add github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass generate [args] [name]".EscapeMarkup(),
            Locale.Get("help.description.generate"),
            "winpass generate -s=12 -a=abc123 -c github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (remove|delete) [name]".EscapeMarkup(),
            Locale.Get("help.description.delete"),
            "winpass remove github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (rename|move) [args] [name]".EscapeMarkup(),
            Locale.Get("help.description.rename"),
            "winpass rename github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (find|search|grep) [text]".EscapeMarkup(),
            Locale.Get("help.description.find"),
            "winpass find \"email: my-email@github.com\""
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass git [args]".EscapeMarkup(),
            Locale.Get("help.description.git"),
            "winpass git status"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass help",
            Locale.Get("help.description.help"),
            "winpass help"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass version",
            Locale.Get("help.description.version"),
            "winpass version"
        );

        AnsiConsole.Write(table);
    }

    private void Edit(IReadOnlyList<string> args)
    {
        if (!AcquireLock()) return;
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

        var (password, error) = AppService.Instance.GetPassword(name, onlyMetadata: true);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (password is null) return;

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
                        Locale.Get("saveAndQuit"),
                        Locale.Get("thePassword"),
                        Locale.Get("theMetadata"),
                        Locale.Get("cancel")
                    )
            );

            if (choice == Locale.Get("saveAndQuit"))
            {
                var (_, errorEditPassword) = AppService.Instance.EditPassword(name, password);

                AnsiConsole.MarkupLine(
                    errorEditPassword is not null
                        ? $"[{GetErrorColor(errorEditPassword.Severity)}]{errorEditPassword.Message}[/]"
                        : $"[green]{Locale.Get("changesSaved")}[/]");

                break;
            }

            if (choice == Locale.Get("cancel")) break;
            if (choice == Locale.Get("thePassword"))
            {
                var newPassword = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter the new password: ")
                        .Secret(null)
                );
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    lastErrorMessage = "Password can't be empty";
                    continue;
                }

                var newPasswordConfirm = AnsiConsole.Prompt(
                    new TextPrompt<string>("Confirm the new password: ")
                        .Secret(null)
                );
                if (!newPassword.Equals(newPasswordConfirm))
                {
                    lastErrorMessage = "Passwords don't match";
                    continue;
                }

                password.Value = newPassword;
                continue;
            }

            if (choice == Locale.Get("theMetadata"))
            {
                Dictionary<string, int> items = new();
                for (var i = 0; i < password.Metadata.Count; ++i)
                {
                    if (password.Metadata[i].Type != MetadataType.Normal) continue;

                    items.Add($"[green]{password.Metadata[i].Key}[/]: {password.Metadata[i].Value}", i);
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
                    password.Metadata.Add(new Metadata(newKey, newValue));
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
                    password.Metadata.RemoveAt(index);
                    continue;
                }

                password.Metadata[index].Key = AnsiConsole.Prompt(
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
                        .DefaultValue(password.Metadata[index].Key)
                );
                password.Metadata[index].Value =
                    AnsiConsole.Ask($"{Locale.Get("questions.enterTheValue")}: ", password.Metadata[index].Value);
                break;
            }
        }
    }

    private void Rename(IReadOnlyList<string> args)
    {
        if (!AcquireLock()) return;
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
        var duplicate = false;

        for (var i = 0; i < args.Count - 1; ++i)
        {
            if (args[i] == "-d")
            {
                duplicate = true;
            }
        }

        AnsiConsole.Write($"{Locale.Get("questions.enterNewName")}: ");
        var newName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(newName))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.nameIsEmpty")}[/]");
            return;
        }

        var choice =
            AnsiConsole.Ask(
                Locale.Get("questions.confirmWantsToRenamePassword", new[]
                {
                    duplicate ? Locale.Get("duplicate") : Locale.Get("rename"),
                    name,
                    newName
                }),
                Locale.Get("n")).ToLower();

        if (choice != Locale.Get("y")) return;

        var (_, error) = AppService.Instance.RenamePassword(name, newName, duplicate);

        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine(duplicate
            ? $"[green]{Locale.Get("passwordDuplicated", new object[] { name, newName })}[/]"
            : $"[green]{Locale.Get("passwordRenamed", new object[] { name, newName })}[/]");
    }

    private void Delete(IReadOnlyCollection<string> args)
    {
        if (!AcquireLock()) return;
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

        var name = args.Last();

        var choice = AnsiConsole
            .Ask<string>(Locale.Get("questions.confirmDeletePassword", new object[] { name }), "n")
            .ToLower();

        if (choice != Locale.Get("y")) return;

        var (_, error) = AppService.Instance.DeletePassword(name);

        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]{Locale.Get("passwordRemoved", new object[] { name })}[/]");
    }

    private void Generate(IReadOnlyList<string> args)
    {
        if (!AcquireLock()) return;
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
        var copy = true;
        var timeout = settings?.ClearTimeout ?? 10;
        if (timeout <= 0)
        {
            timeout = 10;
        }

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

            if (args[i] == "-c")
            {
                copy = false;
            }

            if (args[i].StartsWith("-t="))
            {
                if (!int.TryParse(args[i].Split("-t=").Last(), out var intValue)) continue;
                timeout = intValue <= 0 ? 10 : intValue;
            }
        }

        var (password, error) = AppService.Instance.GeneratePassword(
            name,
            length,
            customAlphabet,
            copy,
            timeout
        );

        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (copy)
        {
            AnsiConsole.MarkupLine(Locale.Get("passwordGeneratedAndCopied", new object[] { name }));
            AnsiConsole.MarkupLine(
                $"[yellow]{Locale.Get("clipboardWillCleared", new object[] { timeout.ToString() })}[/]");
            return;
        }

        AnsiConsole.MarkupLine(Locale.Get("passwordGenerated", new object[] { name }));

        DisplayPassword(password);
        password = null;
        GC.Collect();
    }

    private void Search(IReadOnlyList<string> args)
    {
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

        var term = args[0];
        List<StoreEntry> entries = new();

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Searching...", _ => { entries = AppService.Instance.Search(term); });

        var tree = new Tree(string.Empty);
        RenderEntries(entries, tree);
        AnsiConsole.Write(tree);
    }

    private void Insert(IReadOnlyList<string> args)
    {
        if (!AcquireLock()) return;

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

        if (AppService.Instance.DoEntryExists(name))
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
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.passwordEmpty")}[/]");
            return;
        }

        var passwordConfirm = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Locale.Get("confirmPassword")}: ")
                .Secret(null)
        );
        if (!password.Equals(passwordConfirm))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.passwordsDontMatch")}[/]");
            return;
        }

        var (_, error) = AppService.Instance.InsertPassword(name, new Password(password));
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine(Locale.Get("passwordCreated", new object[] { name }));
    }

    private void ClearClipboard(IReadOnlyList<string> args)
    {
        if (args.Count == 0) return;
        if (!int.TryParse(args[0], out var intValue)) return;

        Thread.Sleep(1000 * intValue);
        ClipboardHelper.Clear();
    }

    private void Show(List<string> args)
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

        if (copy && dontClear)
        {
            AnsiConsole.MarkupLine($"[yellow]{Locale.Get("argfNoEfectWithArgc")}[/]");
        }

        var (password, error) =
            AppService.Instance.GetPassword(name, copy, timeout, onlyMetadata: !showPassword && showMetadata);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (copy)
        {
            AnsiConsole.MarkupLine($"[green]{Locale.Get("passwordCopied")}[/]");
            AnsiConsole.MarkupLine(
                $"[yellow]{Locale.Get("clipboardWillCleared", new object[] { timeout.ToString() })}[/]");
            return;
        }

        if (password is null) return;

        if (showMetadata && password.Metadata.Any())
        {
            DisplayMetadata(password.Metadata);
        }

        if (!showMetadata || showPassword)
        {
            DisplayPassword(password.Value);
            password.Dispose();
            password = null;
            GC.Collect();

            if (dontClear) return;

            AnsiConsole.MarkupLine(
                $"[yellow]{Locale.Get("terminalWillCleared", new object[] { timeout.ToString() })}[/]");

            Thread.Sleep(timeout * 1000);
            Console.Clear();
        }
    }

    private void List()
    {
        if (!AppService.Instance.IsStoreInitialized())
        {
            AnsiConsole.MarkupLine($"[red]{new FsStoreNotInitializedError().Message}[/]");
            return;
        }

        var (entries, error) = AppService.Instance.ListStoreEntries();
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (entries is null) return;

        var tree = new Tree(string.Empty);
        RenderEntries(entries, tree);
        AnsiConsole.Write(tree);
    }

    private void Init()
    {
        var gpgId = AnsiConsole.Ask<string>($"{Locale.Get("questions.gpgId")}: ");

        var match = RegGpgKeyId.Match(gpgId);
        if (!match.Success || match.Index != 0 || match.Length != gpgId.Length)
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.invalidGpgKey")}[/]");
            return;
        }

        var gitUrl = AnsiConsole.Ask<string>($"{Locale.Get("questions.gitRepositoryUrl")}: ");
        if (string.IsNullOrEmpty(gitUrl))
        {
            AnsiConsole.MarkupLine($"[red]{Locale.Get("error.gitUrlEmpty")}[/]");
            return;
        }

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("blue"))
            .Start("Initializing...", _ =>
            {
                var (_, error) = AppService.Instance.InitializeStoreFolder(gpgId, gitUrl);
                if (error is not null)
                {
                    AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
                    return;
                }

                AnsiConsole.MarkupLine($"[green]{Locale.Get("storeInitialized")}[/]");
            });
    }

    #endregion

    #region Private methods

    private bool AcquireLock()
    {
        if (AppService.Instance.AcquireLock()) return true;

        AnsiConsole.MarkupLine(
            "[red]Unable to acquire lock. There must be another instance of winpass currently running[/]");
        return false;
    }

    private void DisplayMetadata(List<Metadata> metadata)
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
        };
        table.AddColumn(Locale.Get("key"));
        table.AddColumn(Locale.Get("value"));

        foreach (var m in metadata.OrderBy(m => m.Type))
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

    private void DisplayPassword(string value)
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            ShowHeaders = false,
        };

        table.AddColumn(string.Empty);
        table.AddRow($"{Locale.Get("passwordIs")} [yellow]{value.EscapeMarkup()}[/]");
        AnsiConsole.Write(table);
        table.Rows.Clear();
        table = null;
        GC.Collect();
    }

    private void RenderEntries(List<StoreEntry> entries, IHasTreeNodes node)
    {
        foreach (var entry in entries)
        {
            var style = string.Join(
                " ",
                new[]
                {
                    entry.Highlight ? "underline" : string.Empty,
                    entry.IsFolder ? string.Empty : "blue"
                }.Where(v => !string.IsNullOrEmpty(v))
            );
            var n = node.AddNode(
                string.IsNullOrWhiteSpace(style) ? entry.Name : $"[{style}]{entry.Name}[/]"
            );

            foreach (var metadata in entry.Metadata)
            {
                n.AddNode($"[green]{metadata}[/]");
            }

            if (!entry.IsFolder) continue;

            if (!entry.Entries.Any())
            {
                n.AddNode("...");
                continue;
            }

            RenderEntries(entry.Entries, n);
        }
    }

    private string GetErrorColor(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Error => "red",
            ErrorSeverity.Info => "blue",
            ErrorSeverity.Success => "green",
            ErrorSeverity.Warning => "yellow",
            _ => "white",
        };
    }

    #endregion
}