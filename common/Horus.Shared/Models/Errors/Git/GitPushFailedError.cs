using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitPushFailedError : Error
{
    public GitPushFailedError(string message) : base($"Git push failed: {message}")
    {
    }
}