using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsStoreFolderAlreadyExistsError : Error
{
    public FsStoreFolderAlreadyExistsError() : base("Store folder already exists")
    {
    }
}