using Horus.Shared.Enums;
using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgEmptyPasswordError : Error
{
    public GpgEmptyPasswordError() : base("Password is empty", ErrorSeverity.Warning)
    {
    }
}