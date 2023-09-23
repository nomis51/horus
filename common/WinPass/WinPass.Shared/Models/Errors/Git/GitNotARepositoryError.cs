using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitNotARepositoryError : Error
{
    public GitNotARepositoryError() : base("Folder is not a git repository")
    {
    }
}