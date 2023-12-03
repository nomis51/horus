using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgInvalidKeyError : Error
{
    public GpgInvalidKeyError() : base("Invalid GPG key")
    {
    }
}