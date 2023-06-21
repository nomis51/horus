using Newtonsoft.Json;
using Serilog;
using Spectre.Console;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Enums;
using WinPass.Shared.Extensions;
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
            new[]
            {
                "/c",
                "echo",
                value.ToBase64(),
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
            }
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
            : new Result<Settings?, Error?>(JsonConvert.DeserializeObject<Settings>(result.FromBase64()));
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

        if (!result.IsBase64())
        {
            // Handles pass passwords
            var lines = result
                .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim('\r'))
                .ToList();
            if (!lines.Any()) return new Result<Password?, Error?>(new GpgEmptyPasswordError());

            var password = new Password
            {
                Value = lines.First(),
                Metadata = lines.Skip(1)
                    .Select(l =>
                    {
                        var index = l.IndexOf(":", StringComparison.Ordinal);
                        if (index == -1 || index + 1 >= l.Length) return null;

                        Metadata m = new(
                            l[..index],
                            l[(index + 1)..]
                        );
                        return m;
                    })
                    .Where(m => m is not null)
                    .ToList()!
            };

            lines.Clear();
            lines = null;
            GC.Collect();

            if (onlyMetadata)
            {
                password.Dispose();
            }

            return new Result<Password?, Error?>(password);
        }

        try
        {
            var password = JsonConvert.DeserializeObject<Password>(result.FromBase64());
            if (onlyMetadata)
            {
                password!.Dispose();
            }

            return new Result<Password?, Error?>(password);
        }
        catch (Exception e)
        {
            return new Result<Password?, Error?>(new GpgDecryptError(e.Message));
        }
        finally
        {
            result = null;
            GC.Collect();
        }
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
        return new[]
        {
            "echo",
            value.ToBase64(),
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
        };
    }

    #endregion
}