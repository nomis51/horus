using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgDecryptError : Error
{
    public GpgDecryptError(string message) : base($"Unable to decrypt password: {message}")
    {
    }
}