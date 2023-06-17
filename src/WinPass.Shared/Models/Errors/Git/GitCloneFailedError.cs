using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitCloneFailedError : Error
{
    public GitCloneFailedError() : base("Failed to git clone")
    {
    }
}