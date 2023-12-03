using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgNotInstalledError : Error
{
    public GpgNotInstalledError() : base("GPG is not installed")
    {
    }
}