using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitFetchFailedError : Error
{
    public GitFetchFailedError(string message) : base($"Git fetch failed: {message}")
    {
    }
}