using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitCloneFailedError : Error
{
    public GitCloneFailedError() : base("Failed to git clone")
    {
    }
}