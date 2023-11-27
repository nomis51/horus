using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgKeyNotFoundError : Error
{
    public GpgKeyNotFoundError() : base("GPG key not found")
    {
    }
}