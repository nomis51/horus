# Commands
This page will discuss all the commands available in details.

## init
`winpass init`

Initalize the password store in your user home folder. This is located in `C:/Users/<username>/.winpass` on Windows and ~/.winpass on Linux.

The command will ask for two informations :

**GPG ID** : This the ID of the GPG keypair you have generated using GPG.
To get the ID of the GPG keypair simply run `gpg -k` in your terminal and find out your keypair in the list and copy the ID from there. We want the full ID here, not the 8 characters short version. The ID is a 40 hex digits.

**Private remote git repository URL** : The **private** remote git repository where you want to save your password store. This will allow you to share your password store accross multiple machines easily. If you don't want to save your password store in a remote git repository, you can simply create an empty GitHub repository, using it to initialize the password store and then delete the repository and forget about it.

After the password store has been created, you'll find a couple files in the folder : 
- .gpg-id : contains the ID of the GPG keypair used to encrypt the passwords in the password store
- .lock : the lock file used by winpass to prevent multiple instances of winpass from trying to write at the same time in the password store (should not happen, but just in case).
- .gitignore : make git ignore the files listed in the file. Used to ignore the `.lock` file, because it is not usefull to publish in to the git repository since it doesn't contains any useful information and winpass will simply generate a new one if it gets deleted.

### Examples
`winpass init`

## ls | list
`winpass list` or `winpass ls` or `winpass`

List all the entries in the password store in a tree view.

### Examples
`winpass ls`
`winpass list`
`winpass`

## show
`winpass show [args] [name]`

Show informations about the entry in the password store. `[name]` is the name of the entry you wan to display.

There are several arguments you can provide to the command : 
| Argument | Description |
| - | - |
| -m | Show the metadata of the entry. By default if you don't provide any arguments, winpass will execute the equivalent of `winpass show -m [name]` |
| -p | Show the password value of the entry. The terminal will automatically clear after a while since this is a sensitive information |
| -f | Prevent the terminal from clearing after a while if the argument `-p` has been provided. |
| -c | Copy the password value of the entry into your clipboard. The clipboard will automatically clear after a while. Note that `-c` overrides all other arguments and `-f` has no effect on it. |

### Examples
`winpass show facebook` : will show the metadatas of the "facebook" entry.
`winpass show -m facebook` : will show the metadatas of the "facebook" entry.
`winpass show -p facebook` : will show the password of the "facebook" entry and clear the terminal after a while
`winpass show -p -m facebook` : will show the metadatas AND password of the "facebook" entry and clear the terminal after a while
`winpass show -p -m -f facebook` : will show the metadatas AND password of the "facebook" entry and NOT clear the terminal after a while
`winpass show -c facebook` : will copy the password of the "facebook" entry into your clipboard
`winpass get facebook` : shortcut equivalent to `winpass show -c facebook`

## insert
`winpass insert [name]` or `winpass add [name]`

Add a new entry named `[name]` in the password store. The command will then ask you to enter and confirm the password for that entry.

### Examples
`winpass insert facebook` : will insert a new entry named "facebook"
`winpass add facebook` : will insert a new entry named "facebook"

## generate
`winpass generate [name]`

Add a new entry named `[name]` in the password store, but this time the application will generate a strong password automatically for you.

You can provide some arguments to the command : 

| Argument | Description |
| - | - |
| -s=size | Manually specify the size of the password that will be generated |
| -a=alphabet | Manually specify the alphabet that will be use to generate the password |

### Examples
`winpass generate facebook` : will generate a password named "facebook"
`winpass generate -s=30 facebook` : will generate a password named "facebook" of 30 characters long
`winpass generate -a=abc def` : will generate a password named "facebook" containing only the characters "abc"
`winpass generate -a=abc -s=30 facebook` : will generate a password named "facebook" of 30 characters long containing only the characters "abc"

## edit
`winpass edit [name]`

Let you edit the entry named `[name]` in the password store.
You can edit several informations such as :

- The password
  - By entering a new one manually
  - By asking the app to generate a new strong password for you
- The metadata
  - Add a new metadata (key / value)
  - Edit an existing metadata (key / value)
  - Delete an existing metadata

### Examples
`winpass edit facebook` : will edit the entry named "facebook"

## find
`winpass find [text]` or `winpass search [text]` or `winpass grep [text]`

Search the password store for an entry containing `[text]` in its name or a entry with a metadata containing `[text]`. By default it only search in entry names.

There is an argument available for this command : 

| Argument | Description |
| - | - |
| -m | Also perform a search in the metadata of the entries. Note that **this can be way slower**, because the app has to decrypt every entries in the password store, and decryption is a slow process especially noticeable if you have to decrypt many things. For example, on an average machine with 8 cores can take ~10 seconds to search a password store containing 50 entries |

### Examples
`winpass find facebook` : will search for an entry containing "facebook" in its name.
`winpass find -m facebook` : will search for an entry containing "facebook" in its name OR in its metadata.
`winpass search facebook` : Same as `winpass find facebook`
`winpass grep facebook` : Same as `winpass find facebook`

## remove
`winpass remove [name]` or `winpass delete [name]`

Remove the entry named `[name]` from the password store (with confirmation). You can always revert that by using git and revert the commit associated with the deleted entry, such as `winpass git revert [hash]` where `[hash]` is the ID of the commit.

### Examples
`winpass remove facebook` : will remove the entry named "facebook"
`winpass delete facebook` : Same as `winpass remove facebook`

## rename
`winpass rename [name]` or `winpass move [name]`

Rename or duplicate the entry named `[name]` in the password store. The command will ask you to enter the new name for the entry.

There is an argument that you can provide to the command :

| Argument | Description |
| - | - |
| -d | Instead of renaming the entry, it duplicates it. |

### Examples
`winpass rename facebook` : will rename the entry named "facebook" to a new name
`winpass rename -d "facebook"` : will duplicate the entry named "facebook" with a new name
`winpass move facebook` : Same as `winpass rename facebook`

## git
`winpass git status` or `...`

Execute any git commands you want directly into the password store and will output the result in the terminal.

### Examples
`winpass git status` : will output the git status of the password store
`winpass git push` : will save the password store to the remote git repository
`winpass git pull` : will update the local password store with the remote git repository
`winpass git revert 4945db2` : revert the commit "4945db2" of the password store



