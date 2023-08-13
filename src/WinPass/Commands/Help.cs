using Spectre.Console;
using WinPass.Commands.Abstractions;
using WinPass.Shared;

namespace WinPass.Commands;

public class Help : ICommand
{
    #region Public methods

    public void Run(List<string> args)
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

    #endregion
}