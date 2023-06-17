using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitAddFailedError : Error
{
    public GitAddFailedError() : base("Failed to git add")
    {
    }
}