using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsPasswordFileAlreadyExistsError : Error
{
    public FsPasswordFileAlreadyExistsError() : base("Password file already exists")
    {
    }
}