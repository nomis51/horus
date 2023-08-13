using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsTerminateStoreFailedError : Error
{
    public FsTerminateStoreFailedError() : base("Store termination failed")
    {
    }
}