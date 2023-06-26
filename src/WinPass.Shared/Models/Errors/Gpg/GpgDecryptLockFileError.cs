using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgDecryptLockFileError : Error
{
    public GpgDecryptLockFileError() : base("Unable to decrypt lock file")
    {
    }
}