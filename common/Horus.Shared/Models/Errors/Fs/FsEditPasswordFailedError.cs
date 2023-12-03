using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsEditPasswordFailedError : Error
{
    public FsEditPasswordFailedError() : base("Failed to edit password")
    {
    }
}