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
                { "settings.language", "Language of the application" },
                { "questions.language", "Language" },
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
                { "passwordDuplicated", "Password [blue]{0}[/] duplicated to [blue]{1}[/]" },
                { "passwordRenamed", "Password [blue]{0}[/] renamed to [blue]{1}[/]" },
                {
                    "questions.confirmDeletePassword",
                    "Are you sure you want to delete the password [blue]{0}[/]? [blue](y/n)[/]"
                },
                { "passwordRemoved", "Password [blue]{0}[/] removed" },
                { "error.loadingSettings", "Error while loading settings: {0}. Using default settings instead" },
                { "passwordGeneratedAndCopied", "Password for [blue]{0}[/] generated and [yellow]copied[/]" },
                { "clipboardWillCleared", "Clipboard will be cleared in {0} second(s)" },
                { "passwordGenerated", "Password for [blue]{0}[/] generated" },
                { "cli.args.searchTermRequired", "Search term argument required" },
                { "passwordAlreadyExists", "Password for {0} already exists" },
                { "enterPassword", "Enter the new password" },
                { "error.passwordEmpty", "Password can't be empty" },
                { "confirmPassword", "Confirm the new password" },
                { "error.passwordsDontMatch", "Passwords don't match" },
                { "passwordCreated", "Password for [blue]{0}[/] created" },
                { "argfNoEfectWithArgc", "-f has no effect with -c" },
                { "passwordCopied", "Password copied" },
                { "terminalWillCleared", "Terminal will clear in {0} second(s)" },
                { "error.invalidGpgKey", "Invalid GPG key ID provide" },
                { "questions.gpgId", "GPG ID" },
                { "questions.gitRepositoryUrl", "[bold yellow]Private[/] git remote URL (GitHub, GitLab, etc.)" },
                { "error.gitUrlEmpty", "Git URL was empty" },
                { "storeInitialized", "Store initialized" },
                { "key", "Key" },
                { "value", "Value" },
                { "passwordIs", "Password is" },
                { "metadata.created", "created" },
                { "metadata.modified", "modified" },
                { "duplicate", "duplicate" },
                { "rename", "rename" },
                { "y", "y" },
                { "n", "n" },
            }
        },

        {
            French, new Dictionary<string, string>
            {
                { "settings.defaultPasswordLength", "Longueur par défaut des mots de passe générés" },
                { "settings.defaultCustomAlphabet", "Alphabet par défaut pour générer les mots de passe" },
                { "settings.defaultClearTimeout", "Délai par défaut avant d'effacer le terminal ou le presse-papier" },
                { "settings.language", "Langage de l'application" },
                { "questions.language", "Langage" },
                { "save", "Sauvegarder" },
                { "cancel", "Annuler" },
                { "questions.whatToEdit", "Que voulez-vous modifier" },
                { "settings.saved", "Paramètres sauvegardés" },
                { "questions.passwordLength", "Longueur (Entrer 0 pour remettre la valeur par défaut)" },
                { "questions.customAlphabet", "Alphabet (Entrer r pour remettre la valeur par défaut)" },
                { "questions.clearTimeout", "Délai avant d'effacer (Entrer 0 pour remettre la valeur par défaut)" },
                { "version", "Version de winpass" },
                { "help.command", "Commande" },
                { "help.description", "Description" },
                { "help.example", "Exemple" },
                { "help.description.init", "Initialiser le magasin de mots de passe" },
                { "help.description.ls", "Lister les mots de passe" },
                {
                    "help.description.show",
                    "Afficher le mot de passe [name]\n\nArguments:\n".EscapeMarkup() +
                    string.Join("\n",
                        "-c : Copier le mot de passe dans le presse-papier au lieu de l'afficher",
                        "-m : Afficher les métadonnées du mot de passe s'il y a lieu (N'affiche pas le mot de passe)",
                        "-f : Ne pas effacer automatiquement le terminal après un délai",
                        "-p : Afficher le mot de passe même si -m est passé en argument"
                    )
                },
                { "help.description.insert", "Ajouter le mot de passe [name]".EscapeMarkup() },
                {
                    "help.description.generate",
                    "Générer un nouveau mot de passe nommé [name]\n\nArguments:\n".EscapeMarkup() +
                    string.Join("\n",
                        "-s : Longueur du mot de passe (par défaut: 20)",
                        "-a : Alphabet personnalisé pour générer le mot de passe",
                        "-c : Copier le mot de passe dans le presse-papier au lieu de l'afficher"
                    )
                },
                { "help.description.delete", "Supprimer le mot de passe nommé [name]".EscapeMarkup() },
                {
                    "help.description.rename",
                    "Renommer ou dupliquer le mot de passe nommé [name]\n\nArguments:\n".EscapeMarkup() + string.Join(
                        "\n",
                        "-d : Dupliquer le mot de passe nommé [name] au lieu de le renommer".EscapeMarkup()
                    )
                },
                {
                    "help.description.find",
                    "Rechercher des mots de passe ou métadonnées contenant la valeur [text]".EscapeMarkup()
                },
                { "help.description.git", "Exécute une command git sur le dépôt du magasin de mot de passe" },
                { "help.description.help", "Afficher l'aide (cette page)" },
                { "help.description.version", "Afficher la version" },
                { "cli.args.passwordNameRequired", "Le nom du mot de passe est requis en argument" },
                { "questions.whatToEditOn", "Que voulez-vous modifier sur" },
                { "saveAndQuit", "Sauvegarder et quitter" },
                { "thePassword", "Le mot de passe" },
                { "theMetadata", "Les métadonnées" },
                { "changesSaved", "Changements sauvegardés" },
                { "questions.whatMetadataToEdit", "Quelle métadonnée voulez-vous modifier" },
                { "addNewMetadata", "Une nouvelle métadonnée" },
                { "questions.enterTheKey", "Entrer le [green]nom[/]" },
                { "questions.enterTheValue", "Entrer la [green]valeur[/]" },
                { "error.metadataKeyInvalid", "La clé ne peut pas être vide, ni commencer avec un symbole" },
                { "error.metadataNotFound", "Métadonnée introuvable" },
                { "questions.whatToDoWithMetadata", "Que voulez-vous faire la métadonnée" },
                { "edit", "Modifier" },
                { "delete", "Supprimer" },
                { "questions.enterNewName", "Entrer le nouveau nom" },
                { "error.nameIsEmpty", "Le nom était vide" },
                {
                    "questions.confirmWantsToRenamePassword",
                    "Êtes-vous sûr de vouloir [yellow]{0}[/] le mot de passe [blue]{1}[/] vers [yellow]{2}[/]? [blue](o/n)[/]"
                },
                { "passwordDuplicated", "Le mot de passe [blue]{0}[/] a été dupliqué vers [blue]{1}[/]" },
                { "passwordRenamed", "Le mot de passe [blue]{0}[/] a été renomm vers [blue]{1}[/]" },
                {
                    "questions.confirmDeletePassword",
                    "Êtes-vous sûr de vouloir supprimer le mot de passe [blue]{0}[/]? [blue](o/n)[/]"
                },
                { "passwordRemoved", "Le mot de passe [blue]{0}[/] a été supprimé" },
                {
                    "error.loadingSettings",
                    "Une erreur est survenue lors de la lecture des paramètres: {0}. Les paramètres par défaut seront utilisés"
                },
                {
                    "passwordGeneratedAndCopied",
                    "Le mot de passe [blue]{0}[/] a été généré et [yellow]copié[/] dans le presse-papier"
                },
                { "clipboardWillCleared", "Le presse-papier sera vidé dans {0} seconde(s)" },
                { "passwordGenerated", "Le mot de passe [blue]{0}[/] a été généré" },
                { "cli.args.searchTermRequired", "Le terme de recherche est requis en argument" },
                { "passwordAlreadyExists", "Le mot de passe {0} existe déjà" },
                { "enterPassword", "Entrer le mot de passe" },
                { "error.passwordEmpty", "Le mot de passe ne peut pas être vide" },
                { "confirmPassword", "Confirmer le mot de passe" },
                { "error.passwordsDontMatch", "Les mots de passe ne correspondent pas" },
                { "passwordCreated", "Le mot de passe [blue]{0}[/] a été créé" },
                { "argfNoEfectWithArgc", "L'argument -f n'a pas d'effet avec l'argument -c" },
                { "passwordCopied", "Mot de passe copié dans le presse-papier" },
                { "terminalWillCleared", "Le terminal va être effacé dans {0} seconde(s)" },
                { "error.invalidGpgKey", "L'identifiant de la clé GPG est invalide" },
                { "questions.gpgId", "Identifiant de la clé GPG" },
                {
                    "questions.gitRepositoryUrl",
                    "URL du dépôt [bold yellow]privé[/] git distant (GitHub, GitLab, etc.)"
                },
                { "error.gitUrlEmpty", "L'URL du dépôt est vide" },
                { "storeInitialized", "Magasin de mot de passe initialisé" },
                { "key", "Nom" },
                { "value", "Valeur" },
                { "passwordIs", "Le mot de passe est" },
                { "metadata.created", "Créé le" },
                { "metadata.modified", "Modifié le" },
                { "duplicate", "dupliquer" },
                { "rename", "renommer" },
                { "y", "o" },
                { "n", "n" },
            }
        },
        {
            German, new Dictionary<string, string>
            {
            }
        },
    };

    #endregion

    #region Members

    private static string _selectedLanguage = English;

    #endregion

    #region Public methods

    public static void SetLanguage(string value)
    {
        _selectedLanguage = value;
    }

    public static string Get(string name, object[]? args = null, string language = "")
    {
        if (string.IsNullOrEmpty(language))
        {
            language = _selectedLanguage;
        }

        if (!Translations.ContainsKey(language)) return $"('{language}' translations missing)";
        return !Translations[language].ContainsKey(name)
            ? $"('{language}' translation missing for '{name}')"
            : args is null
                ? Translations[language][name]
                : string.Format(Translations[language][name], args);
    }

    #endregion
}