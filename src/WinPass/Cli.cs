using System.Formats.Asn1;
using System.Text.RegularExpressions;
using Spectre.Console;
using WinPass.Core.Services;
using WinPass.Core.WinApi;
using WinPass.Shared.Enums;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Fs;

namespace WinPass;

public class Cli
{
    #region Constants

    private static readonly Regex RegGpgKeyId =
        new("[a-z0-9]{40}|[a-z0-9]{16}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                Init(commandArgs);
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

            case "git":
                Git(commandArgs);
                break;

            case "cc":
                ClearClipboard(commandArgs);
                break;

            default:
                AnsiConsole.MarkupLine("[red]Invalid command[/]");
                break;
        }
    }

    #endregion

    #region Commands

    private void Git(IEnumerable<string> args)
    {
        var (result, error) = AppService.Instance.ExecuteGitCommand(args.ToArray());
        AnsiConsole.WriteLine(result);
        AnsiConsole.WriteLine(error);
    }

    private void Help()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded
        };
        table.AddColumn("Command");
        table.AddColumn("Description");
        table.AddColumn("Example");

        table.AddRow(
            "winpass init [gpg-id]".EscapeMarkup(),
            "Initialize the password store using the GPG ID provided",
            "winpass init A034B347F727EAA5"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (list|ls|*blank*)".EscapeMarkup(),
            "Show the list of passwords in the store",
            "winpass list"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass show [args] [name]".EscapeMarkup(),
            "Show the password requested by [name]\n\nArguments:\n".EscapeMarkup() + string.Join("\n",
                "-c : Copy the password to the clipboard instead of showing it",
                "-m : Also show metadata of the password if any (Dont' show the password)",
                "-f : Don't automatically clear the terminal after a while",
                "-p : Show the password when -m is provided"
            ),
            "winpass show -c -m github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (insert|add) [name]".EscapeMarkup(),
            "Insert a new password named [name]".EscapeMarkup(),
            "winpass add -m github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass generate [args] [name]".EscapeMarkup(),
            "Generate a new password named [name]\n\nArguments:\n".EscapeMarkup() + string.Join("\n",
                "-s : Size of the password (default: 20)",
                "-a : Custom alphabet to generate the password",
                "-c : Copy the password to the clipboard instead of showing it",
                "-f : Don't automatically clear the terminal after a while"
            ),
            "winpass generate -s=12 -a=abc123 -c github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (remove|delete) [name]".EscapeMarkup(),
            "Delete the password named [name]".EscapeMarkup(),
            "winpass remove github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (rename|move) [args] [name]".EscapeMarkup(),
            "Rename or duplicate the password named [name]\n\nArguments:\n".EscapeMarkup() + string.Join("\n",
                "-d : Duplicate the password named [name] instead of renaming it".EscapeMarkup()
            ),
            "winpass generate -s=12 -a=abc123 -c github/work"
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass (find|search|grep) [text]".EscapeMarkup(),
            "Find passwords or metadata containing [text]".EscapeMarkup() +
            "winpass find \"email: my-email@github.com\""
        );
        table.AddRow(string.Empty, string.Empty, string.Empty);
        table.AddRow(
            "winpass help",
            "Show the help (this)",
            "winpass help"
        );

        AnsiConsole.Write(table);
    }

    private void Edit(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Password name argument required[/]");
            return;
        }

        var name = args[^1];

        var (password, error) = AppService.Instance.GetPassword(name);
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
                    .Title($"What do you want to edit on [blue]{name}[/]?")
                    .AddChoices(
                        "Save and quit",
                        "The password",
                        "The metadata",
                        "Cancel"
                    )
            );

            if (choice == "Save and quit")
            {
                var (_, errorEditPassword) = AppService.Instance.EditPassword(name, password);

                AnsiConsole.MarkupLine(
                    errorEditPassword is not null
                        ? $"[{GetErrorColor(errorEditPassword.Severity)}]{errorEditPassword.Message}[/]"
                        : "[green]Changes saved[/]");

                break;
            }

            if (choice == "Cancel") break;
            switch (choice)
            {
                case "The password":
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

                    password.Set(newPassword);
                    continue;
                }
                case "The metadata":
                {
                    var items = password.Metadata.Select(m => $"[green]{m.Key}[/]: {m.Value}").ToList();
                    var metadataChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Which metadata do you want to edit?")
                            .AddChoices(
                                items.Concat(new[] { "Add new metadata", "Cancel" })
                            )
                    );

                    if (metadataChoice == "Cancel") continue;

                    if (metadataChoice == "Add new metadata")
                    {
                        var newKey = AnsiConsole.Ask<string>("Key: ");
                        var newValue = AnsiConsole.Ask<string>("Value: ");
                        password.Metadata.Add(new Metadata(newKey, newValue));
                        continue;
                    }

                    var index = items.IndexOf(metadataChoice);
                    if (index == -1)
                    {
                        lastErrorMessage = "Metadata not found";
                        continue;
                    }

                    var metadataActionChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("What to do you with the metadata?")
                            .AddChoices("Edit", "Delete")
                    );

                    if (metadataActionChoice == "Delete")
                    {
                        password.Metadata.RemoveAt(index);
                        continue;
                    }

                    password.Metadata[index].Key =
                        AnsiConsole.Ask("Enter the [green]key[/]:", password.Metadata[index].Key);
                    password.Metadata[index].Value =
                        AnsiConsole.Ask("Enter the [green]value[/]:", password.Metadata[index].Value);
                    break;
                }
            }
        }
    }

    private void Rename(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Password name argument required[/]");
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

        AnsiConsole.Write("Enter the new name: ");
        var newName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(newName))
        {
            AnsiConsole.MarkupLine("[red]The name was empty[/]");
            return;
        }

        var choice =
            AnsiConsole.Ask<string>(
                $"Are you sure you want to [yellow]{(duplicate ? "duplicate" : "rename")}[/] the password [blue]{name}[/] into [yellow]{newName}[/]? [blue](y/n)[/]",
                "n").ToLower();

        if (choice != "y") return;

        var (_, error) = AppService.Instance.RenamePassword(name, newName, duplicate);

        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine(duplicate
            ? $"[green]Password [blue]{name}[/] duplicated to [blue]{newName}[/][/]"
            : $"[green]Password [blue]{name}[/] renamed to [blue]{newName}[/][/]");
    }

    private void Delete(IReadOnlyCollection<string> args)
    {
        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Password name argument required[/]");
            return;
        }

        var name = args.Last();

        var choice = AnsiConsole
            .Ask<string>($"Are you sure you want to delete the password [blue]{name}[/]? [blue](y/n)[/]", "n")
            .ToLower();

        if (choice != "y") return;

        var (_, error) = AppService.Instance.DeletePassword(name);

        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"[green]Password [blue]{name}[/] removed[/]");
    }

    private void Generate(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Password name argument required[/]");
            return;
        }

        var name = args[^1];
        var length = 0;
        var customAlphabet = string.Empty;
        var copy = true;
        var dontClear = false;
        var timeout = 10;

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

            if (args[i] == "-f")
            {
                dontClear = true;
            }

            if (args[i].StartsWith("-t="))
            {
                if (!int.TryParse(args[i].Split("-t=").Last(), out var intValue)) continue;
                timeout = intValue;
            }
        }

        var (password, error) = AppService.Instance.GeneratePassword(name, length, customAlphabet, copy, timeout);

        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (copy)
        {
            AnsiConsole.MarkupLine($"Password for [blue]{name}[/] generated and [yellow]copied[/]");
            AnsiConsole.MarkupLine($"[yellow]Clipboard will be cleared in {timeout} second(s)[/]");
            return;
        }

        AnsiConsole.MarkupLine($"Password for [blue]{name}[/] generated");

        DisplayPassword(password);
        password = string.Empty;
        GC.Collect();

        if (dontClear) return;
        AnsiConsole.MarkupLine($"[yellow]Terminal will clear in {timeout} second(s)[/]");

        Thread.Sleep(timeout * 1000);
        Console.Clear();
    }

    private void Search(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Search term argument required[/]");
            return;
        }

        var term = args[0];
        var entries = AppService.Instance.Search(term);
        var tree = new Tree(string.Empty);
        RenderEntries(entries, tree);
        AnsiConsole.Write(tree);
    }

    private void Insert(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Password name argument required[/]");
            return;
        }

        var name = args[0];

        if (AppService.Instance.DoEntryExists(name))
        {
            AnsiConsole.MarkupLine($"[red]Password for {name} already exists[/]");
            return;
        }

        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the new password: ")
                .Secret(null)
        );
        if (string.IsNullOrWhiteSpace(password))
        {
            AnsiConsole.MarkupLine("[red]Password can't be empty[/]");
            return;
        }

        var passwordConfirm = AnsiConsole.Prompt(
            new TextPrompt<string>("Confirm the new password: ")
                .Secret(null)
        );
        if (!password.Equals(passwordConfirm))
        {
            AnsiConsole.MarkupLine("[red]Passwords don't match[/]");
            return;
        }

        var (_, error) = AppService.Instance.InsertPassword(name, password);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine($"Password for [blue]{name}[/] created");
    }

    private void ClearClipboard(IReadOnlyList<string> args)
    {
        if (args.Count == 0) return;
        if (!int.TryParse(args[0], out var intValue)) return;

        Thread.Sleep(1000 * intValue);
        User32.ClearClipboard();
    }

    private void Show(List<string> args)
    {
        if (args.Count == 0 || string.IsNullOrEmpty(args[0]))
        {
            AnsiConsole.MarkupLine("[red]password name argument required[/]");
            return;
        }

        var name = args[^1];
        args.RemoveAt(args.Count - 1);

        var copy = false;
        var dontClear = false;
        var showMetadata = false;
        var showPassword = false;
        var timeout = 10;

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
                        timeout = intValue;
                    }

                    break;
            }
        }

        if (copy && dontClear)
        {
            AnsiConsole.MarkupLine("[yellow]-f has no effect with -c[/]");
        }

        var (password, error) = AppService.Instance.GetPassword(name, copy, timeout);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        if (copy)
        {
            AnsiConsole.MarkupLine("[green]Password copied[/]");
            AnsiConsole.MarkupLine($"[yellow]Clipboard will be cleared in {timeout} seconds[/]");
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

            if (dontClear) return;

            AnsiConsole.MarkupLine($"[yellow]Terminal will clear in {timeout} seconds[/]");

            Thread.Sleep(timeout * 1000);
            Console.Clear();
        }
    }

    private void List()
    {
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

    private void Init(IReadOnlyList<string> args)
    {
        var gpgId = AnsiConsole.Ask<string>("GPG ID: ");

        var match = RegGpgKeyId.Match(gpgId);
        if (!match.Success || match.Index != 0 || match.Length != gpgId.Length)
        {
            AnsiConsole.MarkupLine("[red]Invalid GPG key ID provided[/]");
            return;
        }

        var gitUrl = AnsiConsole.Ask<string>("[bold yellow]Private[/] git remote URL (GitHub, GitLab, etc.): ");
        if (string.IsNullOrEmpty(gitUrl))
        {
            AnsiConsole.MarkupLine("[red]Git URL was empty[/]");
            return;
        }

        var (_, error) = AppService.Instance.InitializeStoreFolder(gpgId, gitUrl);
        if (error is not null)
        {
            AnsiConsole.MarkupLine($"[{GetErrorColor(error.Severity)}]{error.Message}[/]");
            return;
        }

        AnsiConsole.MarkupLine("[green]Store initialized[/]");
    }

    #endregion

    #region Private methods

    private void DisplayMetadata(List<Metadata> metadata)
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
        };
        table.AddColumn("Key");
        table.AddColumn("Value");

        foreach (var m in metadata)
        {
            table.AddRow(m.Key, m.Value);
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
        table.AddRow($"Password is [yellow]{value.EscapeMarkup()}[/]");
        AnsiConsole.Write(table);
        table.Rows.Clear();
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

    private string ReadPassword()
    {
        var password = string.Empty;
        ConsoleKey key;
        AnsiConsole.WriteLine("Enter the password: ");
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && password.Length > 0)
            {
                Console.Write("\b \b");
                password = password[..^1];
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                Console.Write("*");
                password += keyInfo.KeyChar;
            }
        } while (key != ConsoleKey.Enter);

        var (_, top) = Console.GetCursorPosition();
        Console.SetCursorPosition(0, top - 2 < 0 ? 0 : top - 2);

        for (var i = 0; i < 2; ++i)
        {
            for (var k = 0; k < Console.WindowWidth; ++k)
            {
                Console.Write(" ");
            }
        }

        return password;
    }

    #endregion
}