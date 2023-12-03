using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsStoreNotInitializedError : Error
{
    public FsStoreNotInitializedError() : base("Store is not initialized")
    {
    }
}