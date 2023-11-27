using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitAddFailedError : Error
{
    public GitAddFailedError() : base("Failed to git add")
    {
    }
}