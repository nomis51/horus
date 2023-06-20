using Newtonsoft.Json;
using Serilog;
using Spectre.Console;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Gpg;

namespace WinPass.Core.Services;

public class GpgService : IService
{
    #region Constants

    private const string Gpg = "gpg";

    #endregion

    #region Public methods

    public bool Verify()
    {
        try
        {
            var (ok, result, error) = ProcessHelper.Exec(Gpg, new[] { "--version" });
            return ok && string.IsNullOrEmpty(error) && result.StartsWith("gpg (GnuPG)");
        }
        catch (Exception e)
        {
            Log.Warning("Unable to verify GnuPG installation: {Message}", e.Message);
        }

        return false;
    }

    public ResultStruct<byte, Error?> Encrypt(string key, string filePath, string value)
    {
        var (ok, _, error) = ProcessHelper.Exec(
            "cmd",
            PrepareEncryptArgs(value, key, filePath)
        );
        return !ok ? new ResultStruct<byte, Error?>(new GpgEncryptError(error)) : new ResultStruct<byte, Error?>(0);
    }

    public Result<Settings?, Error?> DecryptSettings(string filePath)
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
        if (!ok) return new Result<Settings?, Error?>(new GpgDecryptError(error));

        return string.IsNullOrWhiteSpace(result)
            ? new Result<Settings?, Error?>(new GpgDecryptError(error))
            : new Result<Settings?, Error?>(JsonConvert.DeserializeObject<Settings>(result));
    }

    public Result<Password?, Error?> DecryptPassword(string filePath, bool onlyMetadata = false)
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
        if (!ok) return new Result<Password?, Error?>(new GpgDecryptError(error));

        if (string.IsNullOrWhiteSpace(result)) return new Result<Password?, Error?>(new GpgDecryptError(error));

        var lines = result
            .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(v => v.Trim('\r'))
            .ToList();
        if (!lines.Any()) return new Result<Password?, Error?>(new GpgEmptyPasswordError());

        Password password = new(onlyMetadata ? string.Empty : lines.First());

        for (var i = 1; i < lines.Count; ++i)
        {
            var parts = lines[i].Split(":", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            password.Metadata.Add(new Metadata(parts[0], string.Join(string.Empty, parts.Skip(1))));
        }

        lines.Clear();
        lines = null;
        GC.Collect();

        return new Result<Password?, Error?>(password);
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

    #region Private methods

    private string[] PrepareEncryptArgs(string value, string key, string filePath)
    {
        var parts = value.Split("\r\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var args = new List<string> { "/c", "(" };

        for (var i = 0; i < parts.Length; ++i)
        {
            args.Add($"echo {parts[i]}");
            if (i + 1 < parts.Length)
            {
                args.Add(" & ");
            }
        }

        args.Add(")");

        return args.Concat(new[]
        {
            "|",
            Gpg,
            "--quiet",
            "--yes",
            "--compress-algo=none",
            "--no-encrypt-to",
            "-e",
            "-r",
            key,
            "-o",
            filePath
        }).ToArray();
    }

    #endregion
}