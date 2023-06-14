using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgEncryptError : Error
{
    public GpgEncryptError(string message) : base($"Unable to encrypt password: {message}")
    {
    }
}