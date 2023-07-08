using Newtonsoft.Json;
using Spectre.Console;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Extensions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Gpg;

namespace WinPass.Core.Services;

public class GpgService : IService
{
    #region Public methods

    public bool Verify()
    {
        return Gpg.Gpg.Verify();
    }

    public ResultStruct<byte, Error?> Encrypt(Gpg.Gpg gpg, string filePath, string value)
    {
        return gpg.Encrypt(filePath, value);
    }

    public Result<Settings?, Error?> DecryptSettings(Gpg.Gpg gpg, string filePath)
    {
        var (data, error) = gpg.Decrypt(filePath);
        if (error is not null) return new Result<Settings?, Error?>(error);

        return string.IsNullOrWhiteSpace(data)
            ? new Result<Settings?, Error?>(error)
            : new Result<Settings?, Error?>(JsonConvert.DeserializeObject<Settings>(data.FromBase64()));
    }

    public ResultStruct<byte, Error?> DecryptLock(Gpg.Gpg gpg, string filePath)
    {
        var (data, error) = gpg.Decrypt(filePath);
        if (error is not null) return new ResultStruct<byte, Error?>(error);
        if (string.IsNullOrWhiteSpace(data)) return new ResultStruct<byte, Error?>(new GpgDecryptLockFileError());

        return data.FromBase64() != FsService.GpgLockContent
            ? new ResultStruct<byte, Error?>(new GpgDecryptLockFileError())
            : new ResultStruct<byte, Error?>(0);
    }

    public Result<Password?, Error?> DecryptPassword(Gpg.Gpg gpg, string filePath, bool onlyMetadata = false)
    {
        var (data, error) = gpg.Decrypt(filePath);
        if (error is not null) return new Result<Password?, Error?>(error);
        if (string.IsNullOrWhiteSpace(data)) return new Result<Password?, Error?>(error);

        if (!data.IsBase64())
        {
            // Handles pass passwords
            var lines = data
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
            var password = JsonConvert.DeserializeObject<Password>(data.FromBase64());
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
            data = null;
            GC.Collect();
        }
    }

    public ResultStruct<bool, Error?> IsKeyValid(Gpg.Gpg gpg)
    {
        return gpg.IsValid();
    }

    public void Initialize()
    {
    }

    #endregion
}