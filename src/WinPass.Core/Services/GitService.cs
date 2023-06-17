using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Git;

namespace WinPass.Core.Services;

public class GitService : IService
{
    #region Constants

    private const string Git = "git";

    #endregion

    #region Public methods

    public void Execute(string[] args)
    {
        ProcessHelper.Exec(Git, args, AppService.Instance.GetStorePath());
    }

    public ResultStruct<byte, Error?> Commit(string message)
    {
        var (okAdd, _, errorAdd) = ProcessHelper.Exec(Git, new[] { "add", "." });
        if (!okAdd || !string.IsNullOrEmpty(errorAdd)) return new ResultStruct<byte, Error?>(new GitAddFailedError());

        var (okCommit, _, errorCommit) = ProcessHelper.Exec(Git, new[] { "commit", "-m", $"\"{message}\"" });
        if (!okCommit || !string.IsNullOrEmpty(errorCommit))
            return new ResultStruct<byte, Error?>(new GitCommitFailedError());

        return new ResultStruct<byte, Error?>(0);
    }

    public void Initialize()
    {
    }

    #endregion
}