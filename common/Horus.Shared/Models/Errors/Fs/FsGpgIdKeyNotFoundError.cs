using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsGpgIdKeyNotFoundError : Error
{
    public FsGpgIdKeyNotFoundError() : base("GPG key ID not found")
    {
    }
}