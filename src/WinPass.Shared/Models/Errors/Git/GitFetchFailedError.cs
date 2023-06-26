using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitFetchFailedError : Error
{
    public GitFetchFailedError(string message) : base($"Git fetch failed: {message}")
    {
    }
}