using WinPass.Shared.Enums;
using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Gpg;

public class GpgEmptyPasswordError : Error
{
    public GpgEmptyPasswordError() : base("Password is empty", ErrorSeverity.Warning)
    {
    }
}