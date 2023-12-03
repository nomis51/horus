using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgDecryptLockFileError : Error
{
    public GpgDecryptLockFileError() : base("Unable to decrypt lock file")
    {
    }
}