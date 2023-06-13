using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgDecryptError : Error
{
    public GpgDecryptError() : base("Unable to decrypt password")
    {
    }
}