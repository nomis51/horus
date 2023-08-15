![WinPass logo](https://github.com/nomis51/winpass/blob/master/.assets/winpass.png)

A Windows / Linux password manager greatly inspired by [zx2c4 password-store](https://www.passwordstore.org/).

## Why?
There are several implementations of `pass` targeting different platforms and needs, but the ones supporting Windows are either oudated, unmaintained, buggy or doesn't follow the philosophy of `pass` which is, being a simple, but efficient **terminal-based** password manager. This is why I created `winpass` : a pass-inspired application, zero configuration, terminal-based Windows and Linux supported password manager, improving on the existing recipe defined by zx2c4.

## Installation
- Have [Git](https://git-scm.com/download/win) installed
- Have [GnuPG for Windows](https://gnupg.org/download/) or GPG (linux) installed
- Have [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) installed (for linux follow the instructions [here](https://learn.microsoft.com/en-us/dotnet/core/install/linux) or use `pacman` on Arch)
- Go to the [Release](https://github.com/nomis51/winpass/releases/latest) section
- Download the `WinPass.zip` file for your OS (e.g. `WinPass-win.zip` for Windows or `WinPass-linux.zip` for Linux)
- Extract anywhere you want
- Execute `./winpass version`
- Enjoy!

Upon the first execution, `winpass` will add itself to the `PATH` variable to make it globally available in the terminal. (You might need to restart your terminal)

## Get started
- Make sure you've created or imported your GPG keypair
- Make sure you have a **private** remote git repository created (e.g. GitHub, GitLab, etc.)
- Make sure you have authenticated to that remote git repository (SSH, GPG, [GitHub CLI](https://cli.github.com/manual/installation), etc.)
- Run `winpass init` and follow the instructions

## Update
`winpass` will tell you if there's a new update available.

To proceed, simply download the new version on [GitHub](https://github.com/nomis51/winpass/releases/latest) and replace the content of your *old* folder with the new update.

## Usage
You can execute the command `winpass help` or navigate to the [commands' page]() to have all the information about how the application works and what commands are available

## TODOs
- Add an `import` command 
  - to import backups from the `export` command
  - to import passwords from `pass`
  - to import passwords from a CSV file


# Support
Feel free to open issues if you find any problem or join the [Discord](https://discord.gg/yqDHrqCDq4) server.



