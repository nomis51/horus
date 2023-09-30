using WinPass.Shared.Enums;
using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsStoreAlreadyInitializedError : Error
{
    public FsStoreAlreadyInitializedError() : base("Store already initialized", ErrorSeverity.Warning)
    {
    }
}