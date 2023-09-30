using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsEditPasswordFailedError : Error
{
    public FsEditPasswordFailedError() : base("Failed to edit password")
    {
    }
}