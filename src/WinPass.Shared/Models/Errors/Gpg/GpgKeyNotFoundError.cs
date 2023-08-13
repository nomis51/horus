using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgKeyNotFoundError : Error
{
    public GpgKeyNotFoundError() : base("GPG key not found")
    {
    }
}