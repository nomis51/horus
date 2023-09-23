using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgInvalidKeyError : Error
{
    public GpgInvalidKeyError() : base("Invalid GPG key")
    {
    }
}