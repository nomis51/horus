using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitCommitFailedError : Error
{
    public GitCommitFailedError() : base("Failed to git commit")
    {
    }
}