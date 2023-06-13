using WinPass.Shared.Enums;
using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsStoreNotInitializedError : Error
{
    public FsStoreNotInitializedError() : base("Store is not initialized", ErrorSeverity.Normal)
    {
    }
}