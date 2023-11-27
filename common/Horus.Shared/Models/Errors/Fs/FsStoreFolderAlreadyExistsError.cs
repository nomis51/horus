using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsStoreFolderAlreadyExistsError : Error
{
    public FsStoreFolderAlreadyExistsError() : base("Store folder already exists")
    {
    }
}