using System.Security;
using Spectre.Console;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;

namespace WinPass.Core.Services;

public class GpgService : IService
{
    #region Constants

    private const string Gpg = "gpg";

    #endregion

    #region Public methods

    public Password? Decrypt(string filePath)
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
            return default;
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var lines = result.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (!lines.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Entry is empty[/]");
                return default;
            }

            Password password = new(lines.First());

            for (var i = 1; i < lines.Length; ++i)
            {
                var parts = lines[i].Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                password.Metadata.Add(new Metadata(parts[0], string.Join(string.Empty, parts.Skip(1))));
            }

            return password;
        }

        AnsiConsole.WriteLine($"[red]Error occured while decrypting password: {error}[/]");
        return default;
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