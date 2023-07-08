using System.Runtime.InteropServices;
using Serilog;
using WinPass.Shared.Extensions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Gpg;

namespace WinPass.Gpg;

public class Gpg
{
    #region Constants

    private const string GpgProcessName = "gpg";
    private const string CmdProcessName = "cmd";

    #endregion

    #region Members

    private readonly string _keyId;

    #endregion

    #region Constructors

    public Gpg(string keyId)
    {
        _keyId = keyId;
    }

    #endregion

    #region Public methods

    public static bool Verify()
    {
        try
        {
            var (ok, result, error) = ProcessHelper.Exec(
                GpgProcessName,
                new[] { "--version" }
            );
            return ok && string.IsNullOrEmpty(error) && result.StartsWith("gpg (GnuPG)");
        }
        catch (Exception e)
        {
            Log.Warning("Unable to verify GnuPG installation: {Message}", e.Message);
        }

        return false;
    }

    public ResultStruct<byte, Error?> Encrypt(string filePath, string value)
    {
        var args = new[]
        {
            value.ToBase64(),
            "|",
            GpgProcessName,
            "--quiet",
            "--yes",
            "--compress-algo=none",
            "--no-encrypt-to",
            "-e",
            "-r",
            _keyId,
            "-o",
            filePath
        };
        var (ok, _, error) = ProcessHelper.Exec(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? CmdProcessName : "echo",
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[]
                    {
                        "/c",
                        "echo",
                    }
                    .Concat(args)
                    .ToArray()
                : args
        );
        return !ok ? new ResultStruct<byte, Error?>(new GpgEncryptError(error)) : new ResultStruct<byte, Error?>(0);
    }

    public Result<string, Error?> Decrypt(string filePath)
    {
        var (ok, result, error) = ProcessHelper.Exec(GpgProcessName, new[]
        {
            "--quiet",
            "--yes",
            "--compress-algo=none",
            "--no-encrypt-to",
            "-d",
            filePath
        });
        return !ok ? new Result<string, Error?>(new GpgDecryptError(error)) : new Result<string, Error?>(result);
    }

    public ResultStruct<bool, Error?> IsValid()
    {
        var (ok, result, error) = ProcessHelper.Exec(
            GpgProcessName,
            new[]
            {
                "--list-keys",
                _keyId
            }
        );
        if (error.StartsWith("gpg: error reading key: No public key"))
            return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());
        if (!ok || !string.IsNullOrEmpty(error) || !result.StartsWith("pub"))
            return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());

        const string expireTag = "[E]";
        const string expireLabel = "[expires: ";
        var expireLine = result.Split("\r\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(l => l.Contains(expireTag));
        if (string.IsNullOrEmpty(expireLine)) return new ResultStruct<bool, Error?>(new GpgInvalidKeyError());

        if (!expireLine.Contains(expireLabel)) return new ResultStruct<bool, Error?>(true);

        var startIndex = expireLine.IndexOf(expireLabel, StringComparison.Ordinal);
        if (startIndex == -1) return new ResultStruct<bool, Error?>(new GpgInvalidKeyError());

        startIndex += expireLabel.Length;

        var endIndex = expireLine.LastIndexOf("]", StringComparison.Ordinal);
        if (endIndex == -1 || endIndex <= startIndex) return new ResultStruct<bool, Error?>(new GpgInvalidKeyError());

        var date = expireLine[startIndex..endIndex];
        return !DateTime.TryParse(date, out var dateTime)
            ? new ResultStruct<bool, Error?>(new GpgInvalidKeyError())
            : new ResultStruct<bool, Error?>(dateTime > DateTime.Now);
    }

    #endregion
}