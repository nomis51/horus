using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Gpg;

public class GpgListIdsError : Error
{
    public GpgListIdsError() : base("Unable to list available GPG IDs")
    {
    }
}