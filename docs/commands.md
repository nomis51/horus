# Commands
This page will discuss all the commands available in details.

## init
`horus init`

Initalize the password store in your user home folder. This is located in `C:/Users/<username>/.horus` on Windows and ~/.horus on Linux.

The command will ask for two informations :

**GPG ID** : This the ID of the GPG keypair you have generated using GPG.
To get the ID of the GPG keypair simply run `gpg -k` in your terminal and find out your keypair in the list and copy the ID from there. We want the full ID here, not the 8 characters short version. The ID is a 40 hex digits.

**Private remote git repository URL** : The **private** remote git repository where you want to save your password store. This will allow you to share your password store accross multiple machines easily. If you don't want to save your password store in a remote git repository, you can simply create an empty GitHub repository, using it to initialize the password store and then delete the repository and forget about it.

After the password store has been created, you'll find a couple files in the folder : 
- .gpg-id : contains the ID of the GPG keypair used to encrypt the passwords in the password store
- .lock : the lock file used by horus to prevent multiple instances of horus from trying to write at the same time in the password store (should not happen, but just in case).
- .gitignore : make git ignore the files listed in the file. Used to ignore the `.lock` file, because it is not usefull to publish in to the git repository since it doesn't contains any useful information and horus will simply generate a new one if it gets deleted.

### Examples
`horus init`

## ls | list
`horus list` or `horus ls` or `horus`

List all the entries in the password store in a tree view.

### Examples
`horus ls`

`horus list`

## show
`horus show [args] [name]`

Show informations about the entry in the password store. `[name]` is the name of the entry you wan to display.

There are several arguments you can provide to the command : 
| Argument | Description |
| - | - |
| -m | Show the metadata of the entry. By default if you don't provide any arguments, horus will execute the equivalent of `horus show -m [name]` |
| -p | Show the password value of the entry. The terminal will automatically clear after a while since this is a sensitive information |
| -f | Prevent the terminal from clearing after a while if the argument `-p` has been provided. |
| -c | Copy the password value of the entry into your clipboard. The clipboard will automatically clear after a while. Note that `-c` overrides all other arguments and `-f` has no effect on it. |

### Examples
`horus show facebook` : will show the metadatas of the "facebook" entry.

`horus show -m facebook` : will show the metadatas of the "facebook" entry.

`horus show -p facebook` : will show the password of the "facebook" entry and clear the terminal after a while

`horus show -p -m facebook` : will show the metadatas AND password of the "facebook" entry and clear the terminal after a while

`horus show -p -m -f facebook` : will show the metadatas AND password of the "facebook" entry and NOT clear the terminal after a while

`horus show -c facebook` : will copy the password of the "facebook" entry into your clipboard

`horus get facebook` : shortcut equivalent to `horus show -c facebook`

## insert
`horus insert [name]` or `horus add [name]`

Add a new entry named `[name]` in the password store. The command will then ask you to enter and confirm the password for that entry.

### Examples
`horus insert facebook` : will insert a new entry named "facebook"

`horus add facebook` : will insert a new entry named "facebook"

## generate
`horus generate [name]`

Add a new entry named `[name]` in the password store, but this time the application will generate a strong password automatically for you.

You can provide some arguments to the command : 

| Argument | Description |
| - | - |
| -s=size | Manually specify the size of the password that will be generated |
| -a=alphabet | Manually specify the alphabet that will be use to generate the password |

### Examples
`horus generate facebook` : will generate a password named "facebook"

`horus generate -s=30 facebook` : will generate a password named "facebook" of 30 characters long

`horus generate -a=abc def` : will generate a password named "facebook" containing only the characters "abc"

`horus generate -a=abc -s=30 facebook` : will generate a password named "facebook" of 30 characters long containing only the characters "abc"

## edit
`horus edit [name]`

Let you edit the entry named `[name]` in the password store.
You can edit several informations such as :

- The password
  - By entering a new one manually
  - By asking the app to generate a new strong password for you
    - You can then see the newly generated password, edit the alphabet or the length, copy it, copy your old password and keep on generating a new password you're happy with the result
- The metadata
  - Add a new metadata (key / value)
  - Edit an existing metadata (key / value)
  - Delete an existing metadata

### Examples
`horus edit facebook` : will edit the entry named "facebook"

## find
`horus find [text]` or `horus search [text]` or `horus grep [text]`

Search the password store for an entry containing `[text]` in its name or a entry with a metadata containing `[text]`. By default it only search in entry names.

There is an argument available for this command : 

| Argument | Description                                              |
| - |----------------------------------------------------------|
| -m | Also perform a search in the metadata of the entries. ** |

** Note that performing a search on the metadata **can be way slower**, because the application has to decrypt every entries' metadata in the password store, and decryption is a slow process. For example, on an average machine with 8 cores it can take ~300ms per entry, so with password store containing 50 entries, you can expect this command to take ~15 seconds to run.

### Examples
`horus find facebook` : will search for an entry containing "facebook" in its name.

`horus find -m facebook` : will search for an entry containing "facebook" in its name OR in its metadata.

`horus search facebook` : Same as `horus find facebook`

`horus grep facebook` : Same as `horus find facebook`

## remove
`horus remove [name]` or `horus delete [name]`

Remove the entry named `[name]` from the password store (with confirmation). You can always revert that by using git and revert the commit associated with the deleted entry, such as `horus git revert [hash]` where `[hash]` is the ID of the commit.

### Examples
`horus remove facebook` : will remove the entry named "facebook"
`horus delete facebook` : Same as `horus remove facebook`

## rename
`horus rename [name]` or `horus move [name]`

Rename or duplicate the entry named `[name]` in the password store. The command will ask you to enter the new name for the entry.

There is an argument that you can provide to the command :

| Argument | Description |
| - | - |
| -d | Instead of renaming the entry, it duplicates it. |

### Examples
`horus rename facebook` : will rename the entry named "facebook" to a new name

`horus rename -d "facebook"` : will duplicate the entry named "facebook" with a new name

`horus move facebook` : Same as `horus rename facebook`

## interact
`horus interact` or `horus interactive` or simply `horus`

Launch the app into interactive mode. Let you run multiple commands in the same session.

## git
`horus git status` or `...`

Execute any git commands you want directly into the password store and will output the result in the terminal.

### Examples
`horus git status` : will output the git status of the password store
`horus git push` : will save the password store to the remote git repository
`horus git pull` : will update the local password store with the remote git repository
`horus git revert 4945db2` : revert the commit "4945db2" of the password store

## config
`horus config`

Let you edit the settings of the application. Settings are stored in the environment variable `horus_SETTINGS`.

Here are the settings you can configure : 

| Setting | Description |
| - | - |
| Language | Change the language of the application from : English (default), french or german |
| Custom alphabet | Set a custom alphabet to generate the passwords. That allow you to not have to provide the argument `-a` everytime you want to generate a new password |
| Custom length | Set a custom length for the generated passwords (default 20). |
| Clear timeout | Set the delay (in seconds) before the terminal or clipboard gets cleared automatically after reading a password with the command `show` (default 10) |
| Passphrase cache timeout | Set the `max-cache-ttl` and `default-cache-ttl` GPG agent settings to the value provided and also adds the `no-allow-external-cache` setting to prevent external keychain/credentials store such as GNOME keychain or Windows credentials manager from caching the passphrase |

### Suggestion
It is suggested that you set a low value to the passphrase cache timeout. That also prevent external credentials manager from caching your passphrase without your confirmation. Something like 30 to 60 seconds is great, since by default GPG cache your passphrase for 600 seconds (10 minutes) which can be unsecure since any call to GPG would simply decrypt the data without confirmation. By keeping this value low, you'll keep your password store convenient to use while being more secure. **Although, don't put this value too low**,  because GPG might then ask for your passphrase multiple times in a row when performing commands such as `find -m` where it needs to decrypt multiple pieces of information in a short period of time, that would make the experience pretty inconvenient.

### Examples
`horus config` : will open the config menu

A passphrase cache timeout of 30 means that GPG will cache your passphrase for 30 seconds before asking for it again. For example, that let you call `horus show my-password` as much as as you want within those 30 seconds without having to re-enter your passphrase, which can be useful if you have to access multiple passwords or metadata within a short period of time.

## destroy
`horus destroy`

Deletes the local version of the password store (doesn't alter the remote version in your git repository)

The command will ask you to confirm that you really want to delete the password store twice and will ask you to type the name of the remote git repository as last confirmation. It will also perform a check with the remite git repository to see if you have local changes that haven't been pushed to the remote git repository, before deleting and will ask you if you want to push those changes to the remote git repository before proceeding, which will make sure you don't loose any piece of unsaved information.

### Examples
`horus destroy` : will delete (after 3 confirmations) the local password store

## migrate
`horus migrate`

Let you migrate the entire password store to use a new GPG keypair. 

The command will ask you to provide the ID of the new GPG keypair, and will then proceed to decrypt and re-encrypt every entries in the password with the new GPG keypair.

This operation can take a while if you have a lot of entries in the password store.

### Examples
`horus migrate` : will migrate the entire password store to use a new GPG keypair after confirmation

## export
`horus export`

Export the entire password store into a zip file, ready to be backed up somewhere safe. Note that the entries are **still encrypted** in the zip file and it also contains the git history, if you loose your remote git repository and your local copy, you can always recover everything using that backup zip file.

### Examples
`horus export` : export the entire password store (still encrypted) and the git history

## gpg-...
There are some commands to manage the gpg-agent that are available as a shortcut within the application :

| Command | Description |
| - | - |
| `horus gpg-start-agent` | Starts the GPG agent, if it wasn't already started |
| `horus gpg-stop-agent` | Stops the GPG agent. (On Windows, the agent sometimes gets stuck, so restarting it fixes the issue) |
| `horus gpg-restart-agent` | Shortcut of the two previous commands combined |



