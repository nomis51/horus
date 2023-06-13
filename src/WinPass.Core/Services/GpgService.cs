using System.Security;
using Spectre.Console;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;

namespace WinPass.Core.Services;

public class GpgService : IService
{
    #region Constants

    private const string Gpg = "gpg";

    #endregion

    #region Public methods

    public string Decrypt(string filePath)
    {
        var (ok, result, error) = ProcessHelper.Exec(Gpg, new[]
        {
            "--quiet",
            "--yes",
            "--compress-algo=none",
            "--no-encrypt-to",
            "-d",
            filePath
        });
        if (!ok)
        {
            AnsiConsole.MarkupLine("[red]Unable to decrypt password[/]");
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(result)) return result;

        AnsiConsole.WriteLine($"[red]Error occured while decrypting password: {error}[/]");
        return string.Empty;
    }

    public bool DoKeyExists(string key)
    {
        var (ok, result, error) = ProcessHelper.Exec(Gpg, new[] { "--list-keys", key });
        if (error.StartsWith("gpg: error reading key: No public key")) return false;
        if (ok) return string.IsNullOrEmpty(error) && result.StartsWith("pub");

        AnsiConsole.MarkupLine("[red]Unable to verify GPG key[/]");
        return false;
    }

    public void Initialize()
    {
    }

    #endregion
}