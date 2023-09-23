using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Fs;

public class FsEntryNotFoundError : Error
{
    public FsEntryNotFoundError() : base("Store entry not found")
    {
    }
}