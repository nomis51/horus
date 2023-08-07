using System.Management.Automation;
using Serilog;
using WinPass.Shared.Extensions;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Gpg;

namespace WinPass.Shared.Models;

public class Gpg
{
    #region Constants

    private const string GpgProcessName = "gpg";
    private const string PwshProcessName = "pwsh";

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
            var pwsh = PowerShell.Create();
            pwsh.AddCommand(GpgProcessName);
            pwsh.AddArgument("--version");
            var lines = pwsh.Invoke<string>();

            return lines.FirstOrDefault()?.StartsWith("gpg (GnuPG)") ?? false;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to verify GnuPG installation: {Message}", e.Message);
        }

        return false;
    }

    public ResultStruct<byte, Error?> Encrypt(string filePath, string value)
    {
        var pwsh = PowerShell.Create();
        pwsh.AddCommand(PwshProcessName);
        foreach (var arg in new[]
                 {
                     "-Command",
                     "Write-Output",
                     value.ToBase64(),
                     "|",
                     GpgProcessName,
                     "--quiet",
                     "--yes",
                     "--compress-algo=none",
                     "--no-encrypt-to",
                     "--encrypt",
                     "--recipient",
                     _keyId,
                     "--output",
                     filePath
                 })
        {
            pwsh.AddArgument(arg);
        }

        try
        {
            _ = pwsh.Invoke();
            return new ResultStruct<byte, Error?>(0);
        }
        catch (Exception e)
        {
            return new ResultStruct<byte, Error?>(new GpgDecryptError(e.Message));
        }
    }

    public Result<List<string>, Error?> DecryptMany(IEnumerable<string> filePaths)
    {
        var pwsh = PowerShell.Create();
        pwsh.AddCommand(PwshProcessName);
        pwsh.AddArgument("-Command");
        pwsh.AddArgument("foreach($filePath in (" +
                         string.Join(", ", filePaths) +
                         ")) { gpg --quiet --yes --compress-algo=none --no-encrypt-to --decrypt $filePath; echo \"\" }");

        try
        {
            var result = pwsh.Invoke<string>();
            return new Result<List<string>, Error?>(result.ToList());
        }
        catch (Exception e)
        {
            return new Result<List<string>, Error?>(new GpgDecryptError(e.Message));
        }
    }

    public Result<string, Error?> Decrypt(string filePath)
    {
        var pwsh = PowerShell.Create();
        pwsh.AddCommand(GpgProcessName);
        foreach (var arg in new[]
                 {
                     "--quiet",
                     "--yes",
                     "--compress-algo=none",
                     "--no-encrypt-to",
                     "--decrypt",
                     filePath
                 })
        {
            pwsh.AddArgument(arg);
        }

        try
        {
            var result = pwsh.Invoke<string>();
            return new Result<string, Error?>(string.Join("\n", result));
        }
        catch (Exception e)
        {
            return new Result<string, Error?>(new GpgDecryptError(e.Message));
        }
    }

    public ResultStruct<bool, Error?> IsValid()
    {
        var pwsh = PowerShell.Create();
        pwsh.AddCommand(GpgProcessName);
        pwsh.AddArgument("--list-keys");

        List<string> lines = new();
        try
        {
            lines = pwsh.Invoke<string>().ToList();
        }
        catch (Exception e)
        {
            return new ResultStruct<bool, Error?>(new GpgDecryptError(e.Message));
        }

        if (lines.FirstOrDefault()?.StartsWith("gpg: error reading key: No public key") ?? true)
            return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());
        if (!lines.FirstOrDefault()?.StartsWith("pub") ?? true)
            return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());

        const string expireTag = "[E]";
        const string expireLabel = "[expires: ";
        var expireLine = lines.FirstOrDefault(l => l.Contains(expireTag));
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