using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Git;

public class GitNotInstalledError : Error
{
    public GitNotInstalledError() : base("Git is not installed")
    {
    }
}