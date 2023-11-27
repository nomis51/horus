using Horus.Shared.Enums;
using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsStoreAlreadyInitializedError : Error
{
    public FsStoreAlreadyInitializedError() : base("Store already initialized", ErrorSeverity.Warning)
    {
    }
}