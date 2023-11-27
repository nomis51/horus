using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Fs;

public class FsEntryNotFoundError : Error
{
    public FsEntryNotFoundError() : base("Store entry not found")
    {
    }
}