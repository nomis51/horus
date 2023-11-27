using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitStatusFailedError : Error
{
    public GitStatusFailedError(string message) : base($"Git status failed: {message}")
    {
    }
}