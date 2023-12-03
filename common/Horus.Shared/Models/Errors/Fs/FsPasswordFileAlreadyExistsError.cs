using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsPasswordFileAlreadyExistsError : Error
{
    public FsPasswordFileAlreadyExistsError() : base("Password file already exists")
    {
    }
}