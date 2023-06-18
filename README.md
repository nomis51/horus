# WinPass
A Windows implementation of [zx2c4 password-store](https://www.passwordstore.org/)

## Why?
There are several implementations of `pass` targeting different platforms and needs, but the ones supporting Windows are either oudated, unmaintained, buggy or doesn't follow the philosophy of `pass` which is, being a simple, but efficient **terminal-based** password manager. This is why I created `winpass` : a pass-clone, zero configuration, terminal-based Windows password manager.

## Installation
- Have [Git](https://git-scm.com/download/win) installed
- Have [GnuPG for Windows](https://gnupg.org/download/) installed
- Have [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed
- Go to the [Release](https://github.com/nomis51/winpass/releases/latest) section
- Download the `WinPass.zip` file
- Extract anywhere you want
- Execute `./WinPass.exe version`
- Enjoy!

Upon the first execution, `winpass` will add itself to the Windows `PATH` to make it globally available in the terminal. (You might need to restart your terminal)

## Get started
- Make sure you've created or imported your GPG keys
- Make sure you have a **private** remote git repository created (e.g. GitHub, GitLab, etc.)
- Make sure you have authenticated to that remote git repository (SSH, GPG, GitHub CLI, etc.)
- Run `winpass init` and follow the instructions

## Update
`winpass` will tell you if there's a new update available.

To proceed, simply download the new version on [GitHub](https://github.com/nomis51/winpass/releases/latest) and replace the content of your *old* folder with the new update.

## Usage
You can execute the command `winpass help` to see all the commands available.

| **Command**                | **Description**                                                   | **Arguments**                                                                                                                              | **Example**                                      |
|----------------------------|-------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------|
| init                       | Initialize the password store (in %USERPROFILE%/.password-store/) | None                                                                                                                                       | winpass init                                     |       
| list, ls, (blank)          | Show the list of passwords in the store                           | None                                                                                                                                       | winpass ls                                       |     
| show [args] [name]         | Show / copy the password requested by [name]                      | -c : Copy the password. -m : Show metadata. -f : Don't clear the terminal after a while. -p : Show the password (when -m is also provided) | winpass show -c github/work                      |     
| insert, add [name]         | Insert a new password named [name]                                | None                                                                                                                                       | winpass add github/work                          |     
| generate [args] [name]     | Generate a new password named [name]                              | -s : Size of the password (default : 20). -a : Custom alphabet. -c : Copy the password.                                                    | winpass generate -c -s=12 -a=abc123* github/work |        
| remove, delete [name]      | Remove the password named [name]                                  | None                                                                                                                                       | winpass remove github/work                       |      
| rename, move [args] [name] | Rename or duplicate the pasword named [name]                      | -d : Duplicate the password instead of renaming it                                                                                         | winpass rename -d github/work                    |  
| find, search, grep [text]  | Find passwords or metadata containing [text]                      | None                                                                                                                                       | winpass find "email: my-email@github.com"        |  
| git | Execute git command on the password store repository | Any git arguments | winpass git status |
| version | Show the version | None | winpass version |
| help                       | Show the help                                                     | None                                                                                                                                       | winpass help                                     |




