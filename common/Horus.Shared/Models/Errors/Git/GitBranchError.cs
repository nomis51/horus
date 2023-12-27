using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitBranchError : Error
{
    public GitBranchError(string message) : base($"Failed to git branch: {message}")
    {
    }
}