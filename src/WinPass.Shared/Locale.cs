using Spectre.Console;

namespace WinPass.Shared;

public static class Locale
{
    #region Constants

    public const string English = "en";
    public const string French = "fr";
    public const string German = "de";

    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
    {
        {
            English, new Dictionary<string, string>
            {
                { "settings.defaultPasswordLength", "Default generated password length" },
                { "settings.defaultCustomAlphabet", "Default custom alphabet" },
                { "settings.defaultClearTimeout", "Default clear timeout" },
                { "save", "Save" },
                { "cancel", "Cancel" },
                { "questions.whatToEdit", "What do you want to edit" },
                { "settings.saved", "Settings saved" },
                { "questions.passwordLength", "Length (type 0 to reset to default)" },
                { "questions.customAlphabet", "Alphabet (type r to reset to default)" },
                { "questions.clearTimeout", "Clear timeout (type 0 to reset to default)" },
                { "version", "winpass version" },
                { "help.command", "Command" },
                { "help.description", "Description" },
                { "help.example", "Example" },
                { "help.description.init", "Initialize the password store" },
                { "help.description.ls", "Show the list of passwords in the store" },
                {
                    "help.description.show",
                    "Show the password requested by [name]\n\nArguments:\n".EscapeMarkup() +
                    string.Join("\n",
                        "-c : Copy the password to the clipboard instead of showing it",
                        "-m : Show metadata of the password if any (Don't show the password)",
                        "-f : Don't automatically clear the terminal after a while",
                        "-p : Show the password when -m is provided"
                    )
                },
                { "help.description.insert", "Insert a new password named [name]".EscapeMarkup() },
                {
                    "help.description.generate",
                    "Generate a new password named [name]\n\nArguments:\n".EscapeMarkup() +
                    string.Join("\n",
                        "-s : Size of the password (default: 20)",
                        "-a : Custom alphabet to generate the password",
                        "-c : Copy the password to the clipboard instead of showing it"
                    )
                },
                { "help.description.delete", "Delete the password named [name]".EscapeMarkup() },
                {
                    "help.description.rename",
                    "Rename or duplicate the password named [name]\n\nArguments:\n".EscapeMarkup() + string.Join("\n",
                        "-d : Duplicate the password named [name] instead of renaming it".EscapeMarkup()
                    )
                },
                { "help.description.find", "Find passwords or metadata containing [text]".EscapeMarkup() },
                { "help.description.git", "Execute git command on the password store repository" },
                { "help.description.help", "Show the help (this)" },
                { "help.description.version", "Show the version" },
                { "cli.args.passwordNameRequired", "Password name argument required" },
                { "questions.whatToEditOn", "What do you want to edit on" },
                { "saveAndQuit", "Save and quit" },
                { "thePassword", "The password" },
                { "theMetadata", "The metadata" },
                { "changesSaved", "Changes saved" },
                { "questions.whatMetadataToEdit", "Which metadata do you want to edit" },
                { "addNewMetadata", "Add new metadata" },
                { "questions.enterTheKey", "Enter the [green]key[/]" },
                { "questions.enterTheValue", "Enter the [green]value[/]" },
                { "error.metadataKeyInvalid", "The key cannot be empty or start with a symbol" },
                { "error.metadataNotFound", "Metadata not found" },
                { "questions.whatToDoWithMetadata", "What to do you with the metadata" },
                { "edit", "Edit" },
                { "delete", "Delete" },
                { "questions.enterNewName", "Enter the new name" },
                { "error.nameIsEmpty", "The name was empty" },
                {
                    "questions.confirmWantsToRenamePassword",
                    "Are you sure you want to [yellow]{0}[/] the password [blue]{1}[/] into [yellow]{2}[/]? [blue](y/n)[/]"
                },
                {"passwordDuplicated", "Password [blue]{0}[/] duplicated to [blue]{1}[/]"},
                {"passwordRenamed", "Password [blue]{0}[/] renamed to [blue]{1}[/]"},
                {"questions.confirmDeletePassword", "Are you sure you want to delete the password [blue]{0}[/]? [blue](y/n)[/]"},
                {"passwordRemoved","Password [blue]{0}[/] removed"},
            }
        },

        {
            French, new Dictionary<string, string>
            {
            }
        },
        {
            German, new Dictionary<string, string>
            {
            }
        },
    };

    #endregion

    #region Public methods

    public static string Get(string name, string[]? args = null, string language = English)
    {
        if (!Translations.ContainsKey(language)) return $"('{language}' translations missing)";
        return !Translations[language].ContainsKey(name)
            ? $"('{language}' translation missing for '{name}')"
            : string.Format(Translations[language][name], args);
    }

    #endregion
}