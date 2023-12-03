using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitCommitFailedError : Error
{
    public GitCommitFailedError() : base("Failed to git commit")
    {
    }
}