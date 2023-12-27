using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitCurrentBranchError : Error
{
    public GitCurrentBranchError(string message) : base($"Failed to get current branch: {message}")
    {
    }
}