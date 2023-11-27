using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgEncryptError : Error
{
    public GpgEncryptError(string message) : base($"Unable to encrypt password: {message}")
    {
    }
}