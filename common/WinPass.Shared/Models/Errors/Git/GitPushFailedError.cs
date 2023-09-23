using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitPushFailedError : Error
{
    public GitPushFailedError(string message) : base($"Git push failed: {message}")
    {
    }
}