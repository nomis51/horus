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
                { "settings.passphrasCacheTimeout", "Passphrase cache timeout" },
                { "questions.language", "Language" },
                {"questions.passphrasCacheTimeout", "Passphrase cache timeout (in seconds)"},
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
                { "quit", "Quit" },
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
                { "passwordRemoved", "Password [blue]{0}[/] removed. You can revert this action using git" },
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
                {
                    "questions.confirmDestroyStore",
                    "Are you sure you want to [red]delete[/] completely the current password store"
                },
                { "storeDestroyed", "Store removed" },
                { "repositoryAhead", "You have local changes that haven't been pushed to the remote repository" },
                {
                    "questions.syncChangesBeforeDelete",
                    "Do you want to push those changes before deleting the store? (y/n)"
                },
                // -- 
                { "newVersionAvailable", "[green]New version {0} available! Go to {1} to download the update[/]" },
                { "error.osNotSupported", "[yellow]WinPass only supports Windows and Linux[/]" },
                { "error.occured", "[red]An error occured. Please see the logs at {0}[/]" },
                { "questions.generateNewPassword", "Generate a new password" },
                { "questions.enterPasswordManually", "Enter a new password" },
                { "questions.whatToDo", "What do you want to do?" },
                { "questions.enterNewPassword", "Enter the new password: " },
                { "questions.confirmNewPassword", "Confirm the new password: " },
                { "error.passwordCantEmpty", "Password can't be empty" },
                { "error.passwordDontMatch", "Passwords don't match" },
                { "error.passwordDoesntExists", "Password does not exist" },
                {
                    "questions.confirmMigrateStore",
                    "Are you sure you wan to migrate the store to use the new GPG keypair?"
                },
                { "storeMigrated", "Stored migrated successfully!" },
                { "exportDone", "Store exported to {0} successfully!" },
                { "questions.exportPath", "Enter the path to save the exported store: " },
                {"searching", "Searching"},
                {"mayTakeTime", "May take some time"},
            }
        },

        {
            French, new Dictionary<string, string>
            {
                { "settings.defaultPasswordLength", "Longueur par défaut des mots de passe générés" },
                { "settings.defaultCustomAlphabet", "Alphabet par défaut pour générer les mots de passe" },
                { "settings.defaultClearTimeout", "Délai par défaut avant d'effacer le terminal ou le presse-papier" },
                { "settings.language", "Langage de l'application" },
                { "settings.passphrasCacheTimeout", "Passphrase cache timeout" },
                { "questions.language", "Langage" },
                {"questions.passphrasCacheTimeout", "Passphrase cache timeout (in seconds)"},
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
                { "quit", "Quitter" },
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
                {
                    "passwordRemoved",
                    "Le mot de passe [blue]{0}[/] a été supprimé. Vous pouvez annuler cette opération en utilisant git"
                },
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
                {
                    "questions.confirmDestroyStore",
                    "Êtes-vous sûr de vouloir [red]supprimer[/] complètement le magasin de mot de passe courant"
                },
                { "storeDestroyed", "Magasin de mot de passe supprimé" },
                {
                    "repositoryAhead",
                    "Vous avez des changements locaux qui n'ont pas été sauvegardés sur le dépôt distant"
                },
                {
                    "questions.syncChangesBeforeDelete",
                    "Voulez-vous sauvegarder les changements sur le dépôt distant avant de supprimer le magasin de mot de passe? (o/n)"
                },
                {
                    "newVersionAvailable",
                    "[green]La nouvelle version {0} est maintenant disponible! Naviguer sur {1} pour télécharger la mise à jour[/]"
                },
                { "error.osNotSupported", "[yellow]WinPass ne supporte que Windows et Linux[/]" },
                { "error.occured", "[red]Une erreur est survenue. Veuillez vérifier les logs situé ici {0}[/]" },
                { "questions.generateNewPassword", "Générer une nouveau mot de passe" },
                { "questions.enterPasswordManually", "Entrer une nouveau mot de passe" },
                { "questions.whatToDo", "Que voulez-vous faire?" },
                { "questions.enterNewPassword", "Entrer le nouveau mot de passe: " },
                { "questions.confirmNewPassword", "Confirmer le nouveau mot de passe: " },
                { "error.passwordCantEmpty", "Le mot de passe ne peut pas être vide" },
                { "error.passwordDontMatch", "Les mots de passe de correspondent pas" },
                { "error.passwordDoesntExists", "Le mot de passe n'existe pas" },
                {
                    "questions.confirmMigrateStore",
                    "Êtes-vous sûr de vouloir migrer le magasin de mots de passe vers la nouvelle pair de clés GPG?"
                },
                { "storeMigrated", "Magasin de mots de passe migré avec succès!" },
                { "exportDone", "Magasin de mots de passe exporté vers {0} avec succès!" },
                { "questions.exportPath", "Entrer le chemin où enregistrer l'exportation du magasin de mot de passe: " },
                {"searching", "Recherche"},
                {"mayTakeTime", "Peut prendre un moment"},
            }
        },
        {
            German, new Dictionary<string, string>
            {
                { "settings.defaultPasswordLength", "Die standardkennwortlänge" },
                { "settings.defaultCustomAlphabet", "Das standardmäßige benutzerdefinierte alphabet" },
                { "settings.defaultClearTimeout", "Das standard löschzeitlimit" },
                { "settings.language", "Sprache der bewerbung" },
                { "settings.passphrasCacheTimeout", "Passphrase cache timeout" },
                { "questions.language", "Spracheff" },
                {"questions.passphrasCacheTimeout", "Passphrase cache timeout (in seconds)"},
                { "save", "Speichern" },
                { "cancel", "Stornieren" },
                { "questions.whatToEdit", "Was möchten sie bearbeiten" },
                { "settings.saved", "Einstellungen werden gespeichert" },
                { "questions.passwordLength", "Länge (Geben sie 0 ein auf die standardeinstellungen zurücksetzen)" },
                {
                    "questions.customAlphabet",
                    "Alphabet (Geben sie 'r' ein auf die standardeinstellungen zurücksetzen)"
                },
                {
                    "questions.clearTimeout",
                    "Klare auszeit (Geben sie 0 ein auf die standardeinstellungen zurücksetzen)"
                },
                { "version", "winpass version" },
                { "help.command", "Befehl" },
                { "help.description", "Beschreibung" },
                { "help.example", "Beispiel" },
                { "help.description.init", "Initialisieren sie den passwortspeicher" },
                { "help.description.ls", "Zeigen sie die liste der passwörter im store an" },
                {
                    "help.description.show",
                    "Zeigen sie das von [name] angeforderte passwort an\n\nArgumente:\n".EscapeMarkup() +
                    string.Join("\n",
                        "-c : Kopieren sie das passwort in die zwischenablage, anstatt es anzuzeigen",
                        "-m : Zeigen sie ggf die metadaten des passworts an (passwort nicht anzeigen)",
                        "-f : Leeren sie das terminal nicht automatisch nach einer weile",
                        "-p : Zeigt das passwort an, wenn -m angegeben ist"
                    )
                },
                { "help.description.insert", "Geben sie ein neues passwort mit dem namen [name] ein".EscapeMarkup() },
                {
                    "help.description.generate",
                    "Generieren sie ein neues passwort mit dem namen [name]\n\nArgumente:\n".EscapeMarkup() +
                    string.Join("\n",
                        "-s : Passwortlänge (standard: 20)",
                        "-a : Das benutzerdefinierte alphabet zum generieren des passworts",
                        "-c : Kopieren sie das passwort in die zwischenablage, anstatt es anzuzeigen"
                    )
                },
                { "help.description.delete", "Löschen sie das passwort mit dem namen [name]".EscapeMarkup() },
                {
                    "help.description.rename",
                    "Benennen sie das passwort mit dem namen [name] um oder duplizieren sie es\n\nArgumente:\n"
                        .EscapeMarkup() + string.Join("\n",
                        "-d : Duplizieren sie das passwort mit dem namen [name], anstatt es umzubenennen".EscapeMarkup()
                    )
                },
                { "help.description.find", "Passwörter oder metadaten finden, die [text] enthalten".EscapeMarkup() },
                { "help.description.git", "Führen sie den Git befehl im passwortspeicher repository aus" },
                { "help.description.help", "Zeigen sie die hilfe an (dieses)" },
                { "help.description.version", "Version anzeigen" },
                { "cli.args.passwordNameRequired", "Argument für den passwortnamen erforderlich" },
                { "questions.whatToEditOn", "Was möchten sie bearbeiten" },
                { "quit", "Beenden" },
                { "thePassword", "Das passwort" },
                { "theMetadata", "Die metadaten" },
                { "changesSaved", "Änderungen gespeichert" },
                { "questions.whatMetadataToEdit", "Welche metadaten möchten sie bearbeiten" },
                { "addNewMetadata", "Fügen sie neue metadaten hinzu" },
                { "questions.enterTheKey", "Geben sie den [green]schlüssel[/] ein" },
                { "questions.enterTheValue", "Geben sie den [green]wert[/] ein" },
                { "error.metadataKeyInvalid", "Der schlüssel darf nicht leer sein oder mit einem symbol beginnen" },
                { "error.metadataNotFound", "Metadaten nicht gefunden" },
                { "questions.whatToDoWithMetadata", "Was tun mit den metadaten" },
                { "edit", "Bearbeiten" },
                { "delete", "Löschen" },
                { "questions.enterNewName", "Geben sie den neuen namen ein" },
                { "error.nameIsEmpty", "Der name war leer" },
                {
                    "questions.confirmWantsToRenamePassword",
                    "Sind sie sicher, dass Sie das passwort haben möchten [blue]{1}[/] hinein [yellow]{2}[/]? [blue](j/n)[/]"
                },
                { "passwordDuplicated", "passwort [blue]{0}[/] dupliziert nach [blue]{1}[/]" },
                { "passwordRenamed", "passwort [blue]{0}[/] umbenannt in [blue]{1}[/]" },
                {
                    "questions.confirmDeletePassword",
                    "Sind sie sicher, dass sie das passwort [blue]{0}[/] löschen möchten? [blue](j/n)[/]"
                },
                {
                    "passwordRemoved",
                    "passwort [blue]{0}[/] entfernt. Sie können diese aktion mit git rückgängig machen"
                },
                {
                    "error.loadingSettings",
                    "Fehler beim laden der einstellungen: {0}. Stattdessen werden standardeinstellungen verwendet"
                },
                { "passwordGeneratedAndCopied", "passwort für [blue]{0}[/] generiert und [yellow]kopiert[/]" },
                { "clipboardWillCleared", "Die zwischenablage wird in {0} sekunde(n) geleert" },
                { "passwordGenerated", "passwort für [blue]{0}[/] generiert" },
                { "cli.args.searchTermRequired", "Suchbegriffargument erforderlich" },
                { "passwordAlreadyExists", "Das passwort für {0} existiert bereits" },
                { "enterPassword", "Geben sie das neue passwort ein" },
                { "error.passwordEmpty", "Das passwort darf nicht leer sein" },
                { "confirmPassword", "Bestätigen sie das neue passwort" },
                { "error.passwordsDontMatch", "Passwörter stimmen nicht überein" },
                { "passwordCreated", "passwort für [blue]{0}[/] erstellt" },
                { "argfNoEfectWithArgc", "-f hat keine wirkung mit -c" },
                { "passwordCopied", "passwort kopiert" },
                { "terminalWillCleared", "Das terminal wird in {0} sekunde(n) gelöscht." },
                { "error.invalidGpgKey", "Ungültige GPG-Schlüssel-ID angegeben" },
                { "questions.gpgId", "GPG ID" },
                { "questions.gitRepositoryUrl", "[bold yellow]Private[/] git entfernt URL (GitHub, GitLab, usw.)" },
                { "error.gitUrlEmpty", "Git URL war leer" },
                { "storeInitialized", "Store initialisiert" },
                { "key", "Schlüssel" },
                { "value", "Wert" },
                { "passwordIs", "passwort ist" },
                { "metadata.created", "created" },
                { "metadata.modified", "modified" },
                { "duplicate", "duplikat" },
                { "rename", "umbenennen" },
                { "y", "j" },
                { "n", "n" },
                {
                    "questions.confirmDestroyStore",
                    "Sind sie sicher, dass sie den aktuellen passwortspeicher vollständig [red]entfernen[/] möchten"
                },
                { "storeDestroyed", "passwortspeicher entfernt" },
                {
                    "repositoryAhead",
                    "Sie haben lokale änderungen, die nicht an das remote-repository übertragen wurden"
                },
                {
                    "questions.syncChangesBeforeDelete",
                    "Möchten sie diese änderungen vor dem löschen des shops übernehmen? (j/n)"
                },
                {
                    "newVersionAvailable",
                    "[green]Neue version {0} verfügbar! Gehen sie zu {1}, um das update herunterzuladen[/]"
                },
                { "error.osNotSupported", "[yellow]WinPass unterstützt nur Windows und Linux[/]" },
                {
                    "error.occured",
                    "[red]Es ist ein fehler aufgetreten. Bitte sehen sie sich die protokolle unter {0} an.[/]"
                },
                { "questions.generateNewPassword", "Generieren sie ein neues passwort" },
                { "questions.enterPasswordManually", "Geben sie ein neues kennwort ein" },
                { "questions.whatToDo", "Was möchten sie tun?" },
                { "questions.enterNewPassword", "Geben sie das neue passwort ein: " },
                { "questions.confirmNewPassword", "Bestätigen sie das neue passwort: " },
                { "error.passwordCantEmpty", "Das passwort darf nicht leer sein" },
                { "error.passwordDontMatch", "Passwörter stimmen nicht überein" },
                { "error.passwordDoesntExists", "Das passwort existiert nicht" },
                {
                    "questions.confirmMigrateStore",
                    "Sind sie sicher, dass sie den store migrieren möchten, um das neue GPG-Schlüsselpaar zu verwenden?"
                },
                { "storeMigrated", "Der passwortspeicher wurde erfolgreich migriert!" },
                { "exportDone", "Shop erfolgreich nach {0} exportiert!" },
                { "questions.exportPath", "Geben sie den pfad zum speichern des exportierten passwortspeichers ein" },
                {"searching", "Suchen"},
                {"mayTakeTime", "Kann einige zeit dauern"},
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