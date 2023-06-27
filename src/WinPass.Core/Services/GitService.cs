using System.Text;
using Serilog;
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

    public Result<string, Error?> GetRemoteRepositoryName(string path)
    {
        var gitFolder = Path.Join(path, ".git");
        if (!Directory.Exists(gitFolder)) return new Result<string, Error?>(new GitNotARepositoryError());

        var configFilePath = Path.Join(gitFolder, "config");
        if (!File.Exists(configFilePath)) return new Result<string, Error?>(new GitNotARepositoryError());

        var urlLine = File.ReadAllText(configFilePath)
            .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(l => l.Contains("url = "));
        if (string.IsNullOrEmpty(urlLine)) return new Result<string, Error?>(new GitNotARepositoryError());

        var url = urlLine.Split("=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault() ?? string.Empty;
        if (string.IsNullOrEmpty(url)) return new Result<string, Error?>(new GitNotARepositoryError());

        var index = url.LastIndexOf("/", StringComparison.Ordinal);
        var endIndex = url.LastIndexOf(".git", StringComparison.Ordinal);
        return index >= endIndex
            ? new Result<string, Error?>(new GitNotARepositoryError())
            : new Result<string, Error?>(url[(index + 1)..endIndex]);
    }

    public ResultStruct<byte, Error?> Push(string path)
    {
        var (ok, _, error) = ProcessHelper.Exec(Git, new[] { "push" }, workingDirectory: path);
        return !ok ? new ResultStruct<byte, Error?>(new GitPushFailedError(error)) : new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<bool, Error?> IsAheadOfRemote(string path)
    {
        var (okFetch, _, errorFetch) = ProcessHelper.Exec(Git, new[] { "fetch" }, workingDirectory: path);
        if (!okFetch) return new ResultStruct<bool, Error?>(new GitFetchFailedError(errorFetch));

        var (okStatus, resultStatus, errorStatus) = ProcessHelper.Exec(Git, new[] { "status" }, workingDirectory: path);
        return !okStatus
            ? new ResultStruct<bool, Error?>(new GitStatusFailedError(errorStatus))
            : new ResultStruct<bool, Error?>(resultStatus.Contains("Your branch is ahead of 'origin/master'"));
    }

    public void DeleteRepository(string path)
    {
        var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

        foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
        {
            info.Attributes = FileAttributes.Normal;
        }

        directory.Delete(true);
    }

    public bool Verify()
    {
        try
        {
            var (ok, result, error) = ProcessHelper.Exec(Git, new[] { "--version" });
            return ok && result.StartsWith("git version") && string.IsNullOrEmpty(error);
        }
        catch (Exception e)
        {
            Log.Warning("Unable to verify git installation: {Message}", e.Message);
            return false;
        }
    }

    public bool Clone(string url, string path)
    {
        var tmpPath = Path.GetTempPath();
        var dirName = Path.GetFileName(url).Split(".git").FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrEmpty(dirName)) return false;

        var dirPath = Path.Join(tmpPath, dirName);
        if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);

        var (okClone, _, errorClone) = ProcessHelper.Exec(Git, new[] { "clone", url }, tmpPath);
        if (!okClone || !errorClone.StartsWith("Cloning into"))
        {
            if (Directory.Exists(dirName)) Directory.Delete(dirName, true);
        }

        if (!Directory.Exists(dirPath)) return false;

        Directory.Move(dirPath, path);
        if (!Directory.Exists(Path.Join(path, ".git"))) return false;

        var gpgIdFilePath = Path.Join(path, ".gpg-id");
        if (File.Exists(gpgIdFilePath)) File.Delete(gpgIdFilePath);

        return true;
    }

    public Tuple<string, string> Execute(string[] args)
    {
        var (_, result, error) = ProcessHelper.Exec(Git, args, AppService.Instance.GetStorePath());
        return Tuple.Create(result, error);
    }

    public ResultStruct<byte, Error?> Commit(string message, string path)
    {
        var (okAdd, _, errorAdd) = ProcessHelper.Exec(Git, new[] { "add", "." }, path);
        if (!okAdd || !string.IsNullOrEmpty(errorAdd)) return new ResultStruct<byte, Error?>(new GitAddFailedError());

        var (okCommit, _, errorCommit) = ProcessHelper.Exec(Git, new[] { "commit", "-m", $"\"{message}\"" }, path);
        if (!okCommit || !string.IsNullOrEmpty(errorCommit))
            return new ResultStruct<byte, Error?>(new GitCommitFailedError());

        return new ResultStruct<byte, Error?>(0);
    }

    public void Initialize()
    {
    }

    #endregion
}