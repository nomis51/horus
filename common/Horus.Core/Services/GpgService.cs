using Horus.Core.Services.Abstractions;
using Horus.Shared.Extensions;
using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Errors.Gpg;
using Horus.Shared.Models.Terminal;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Horus.Core.Services;

public class GpgService : IGpgService
{
    #region Constants

    private const string GpgProcessName = "gpg";

    #endregion

    #region Public methods

    public bool Verify()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GpgProcessName,
                    "--version"
                })
                .Execute();
            var ok = result.OutputLines.FirstOrDefault()?.StartsWith("gpg (GnuPG)") ?? false;
            if (!ok) return false;

            new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    "gpg-connect-agent",
                    "reloadagent",
                    "/bye"
                })
                .Execute();

            if (ok)
            {
                StartGpgAgent();
            }

            return ok;
        }
        catch (Exception e)
        {
            Log.Warning("Unable to verify GPG installation: {Message}", e.Message);
            return false;
        }
    }

    public EmptyResult Encrypt(string path, string value, string gpgId = "")
    {
        return EncryptOne(path, value, gpgId);
    }

    public Result<string, Error?> Decrypt(string path)
    {
        return DecryptOne(path);
    }

    public EmptyResult EncryptMetadatas(string path, MetadataCollection metadatas, string gpgId = "")
    {
        return EncryptOne(path, metadatas.ToString(), gpgId);
    }

    public EmptyResult EncryptPassword(string path, Password password, string gpgId = "")
    {
        return EncryptOne(path, password.ToString(), gpgId);
    }

    public Result<MetadataCollection?, Error?> DecryptMetadatas(string path)
    {
        var (data, error) = DecryptOne(path);
        if (error is not null) return new Result<MetadataCollection?, Error?>(error);

        try
        {
            var lstMetadata = JsonSerializer.Deserialize<List<Metadata>>(data);
            return lstMetadata is null
                ? new Result<MetadataCollection?, Error?>(new GpgDecryptError("Resulting data was null"))
                : new Result<MetadataCollection?, Error?>(new MetadataCollection(path, lstMetadata));
        }
        catch (Exception e)
        {
            Log.Error("Unable to deserialize metadatas: {Message}", e.Message);
            return new Result<MetadataCollection?, Error?>(new GpgDecryptError(e.Message));
        }
    }

    public Result<List<MetadataCollection?>, Error?> DecryptManyMetadatas(List<Tuple<string, string>> items)
    {
        var filePaths = items.Select(i => i.Item2);

        List<string> lines = [];
        foreach (var filePath in filePaths)
        {
            var (line, error) = DecryptOne(filePath);
            if (error is not null || string.IsNullOrEmpty(line)) continue;

            lines.Add(line);
        }

        List<MetadataCollection?> results = [];
        for (var i = 0; i < lines.Count; ++i)
        {
            try
            {
                results.Add(
                    new MetadataCollection(
                        items[i].Item1,
                        JsonSerializer.Deserialize<List<Metadata>>(lines[i])!
                    )
                );
            }
            catch (Exception e)
            {
                Log.Error("Unable to deserialize metadatas: {Message}", e.Message);
                results.Add(default);
            }
        }

        return new Result<List<MetadataCollection?>, Error?>(results);
    }

    public Result<Password?, Error?> DecryptPassword(string path)
    {
        var (data, error) = DecryptOne(path);
        return error is not null
            ? new Result<Password?, Error?>(error)
            : new Result<Password?, Error?>(new Password(data));
    }

    public ResultStruct<bool, Error?> IsIdValid(string id = "")
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GpgProcessName,
                    "--list-keys"
                })
                .Execute();

            if (result.OutputLines.FirstOrDefault()?.StartsWith("gpg: error reading key: No public key") ?? true)
            {
                return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());
            }

            if (!result.OutputLines.Any(e => e.StartsWith("pub"))) return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());

            if (string.IsNullOrEmpty(id))
            {
                var (currentId, error) = AppService.Instance.GetStoreId();
                if (error is not null) return new ResultStruct<bool, Error?>(error);
                id = currentId;
            }

            var found = false;
            var index = 0;
            for(; index < result.OutputLines.Count; ++index)
            {
                if (!result.OutputLines[index].StartsWith("pub")) continue;
                ++index;
                
                var lineId = result.OutputLines[index].Trim();
                if (lineId != id) continue;
                
                found = true;
                break;
            }
            
            if(!found) return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());

            index += 2;
            const string expireLabel = "[expires: ";

            var expireLine = result.OutputLines[index];
            if (string.IsNullOrEmpty(expireLine)) return new ResultStruct<bool, Error?>(new GpgInvalidKeyError());

            if (!expireLine.Contains(expireLabel)) return new ResultStruct<bool, Error?>(true);

            var startIndex = expireLine.IndexOf(expireLabel, StringComparison.Ordinal);
            if (startIndex == -1) return new ResultStruct<bool, Error?>(new GpgInvalidKeyError());

            startIndex += expireLabel.Length;

            var endIndex = expireLine.LastIndexOf(']');
            if (endIndex == -1 || endIndex <= startIndex)
                return new ResultStruct<bool, Error?>(new GpgInvalidKeyError());

            var date = expireLine[startIndex..endIndex];
            return !DateTime.TryParse(date, out var dateTime)
                ? new ResultStruct<bool, Error?>(new GpgInvalidKeyError())
                : new ResultStruct<bool, Error?>(dateTime > DateTime.Now);
        }
        catch (Exception e)
        {
            Log.Error("Unable to verify GPG ID: {Message}", e.Message);
            return new ResultStruct<bool, Error?>(new GpgKeyNotFoundError());
        }
    }

    public Result<string, Error?> RestartGpgAgent()
    {
        var (result, error) = StopGpgAgent();
        if (error is not null) return new Result<string, Error?>(error);

        var (result2, error2) = StartGpgAgent();
        if (error2 is not null) return new Result<string, Error?>(error2);

        return new Result<string, Error?>(string.Join("\n", result, result2));
    }

    public Result<string, Error?> StartGpgAgent()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    "gpg-connect-agent",
                    "reloadagent",
                    "/bye"
                })
                .Execute();
            return result.OutputLines.FirstOrDefault(l => l.StartsWith("OK")) is not null
                ? new Result<string, Error?>(string.Join(", ", result.OutputLines))
                : new Result<string, Error?>(new GpgDecryptError("Unable to start GPG agent"));
        }
        catch (Exception e)
        {
            return new Result<string, Error?>(new GpgDecryptError(e.Message));
        }
    }

    public Result<string, Error?> StopGpgAgent()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    "gpgconf",
                    "--kill",
                    "gpg-agent"
                })
                .Execute();
            return new Result<string, Error?>("");
        }
        catch (Exception e)
        {
            return new Result<string, Error?>(new GpgDecryptError(e.Message));
        }
    }

    public void Initialize()
    {
    }

    #endregion

    #region Private methods

    private Result<string, Error?> DecryptOne(string filePath)
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GpgProcessName,
                    "--quiet",
                    "--yes",
                    "--compress-algo=none",
                    "--no-encrypt-to",
                    "--decrypt",
                    filePath
                })
                .Execute();
            result.OutputLines.AddRange(result.ErrorLines);

            return new Result<string, Error?>(
                string.Join(
                    string.Empty,
                    result.OutputLines
                ).FromBase64()
            );
        }
        catch (Exception e)
        {
            Log.Error("Unable to decrypt: {Message}", e.Message);
            return new Result<string, Error?>(new GpgDecryptError(e.Message));
        }
    }

    private EmptyResult EncryptOne(string filePath, string value, string gpgId = "")
    {
        string id;

        if (string.IsNullOrEmpty(gpgId))
        {
            var (gid, error) = AppService.Instance.GetStoreId();
            if (error is not null) return new EmptyResult(error);

            id = gid;
        }
        else
        {
            id = gpgId;
        }

        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    "echo",
                    value.ToBase64()
                })
                .Command(new[]
                {
                    GpgProcessName,
                    "--quiet",
                    "--yes",
                    "--compress-algo=none",
                    "--no-encrypt-to",
                    "--encrypt",
                    "--recipient",
                    id,
                    "--output",
                    filePath
                })
                .Execute();

            if (result.ErrorLines.FirstOrDefault(e => e.Contains("encryption failed")) is not null)
            {
                return new EmptyResult(new GpgEncryptError(string.Join("\n", result.ErrorLines)));
            }

            return new EmptyResult();
        }
        catch (Exception e)
        {
            Log.Error("Unable to encrypt: {Message}", e.Message);
            return new EmptyResult(new GpgEncryptError(e.Message));
        }
    }

    #endregion
}