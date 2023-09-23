using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Git;

public class GitStatusFailedError : Error
{
    public GitStatusFailedError(string message) : base($"Git status failed: {message}")
    {
    }
}