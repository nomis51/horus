using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitNotARepositoryError : Error
{
    public GitNotARepositoryError() : base("Folder is not a git repository")
    {
    }
}