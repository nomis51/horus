using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitNotInstalledError : Error
{
    public GitNotInstalledError() : base("Git is not installed")
    {
    }
}