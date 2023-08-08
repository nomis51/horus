using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsGpgIdKeyNotFoundError : Error
{
    public FsGpgIdKeyNotFoundError() : base("GPG key ID not found")
    {
    }
}