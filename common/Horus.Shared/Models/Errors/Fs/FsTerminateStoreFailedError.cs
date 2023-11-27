using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsTerminateStoreFailedError : Error
{
    public FsTerminateStoreFailedError() : base("Store termination failed")
    {
    }
}