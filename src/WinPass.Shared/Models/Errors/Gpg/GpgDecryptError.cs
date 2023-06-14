using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgDecryptError : Error
{
    public GpgDecryptError(string message) : base($"Unable to decrypt password: {message}")
    {
    }
}