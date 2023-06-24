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

    public ResultStruct<byte, Error?> Commit(string message)
    {
        var storePath = AppService.Instance.GetStorePath();
        var (okAdd, _, errorAdd) = ProcessHelper.Exec(Git, new[] { "add", "." }, storePath);
        if (!okAdd || !string.IsNullOrEmpty(errorAdd)) return new ResultStruct<byte, Error?>(new GitAddFailedError());

        var (okCommit, _, errorCommit) = ProcessHelper.Exec(Git, new[] { "commit", "-m", $"\"{message}\"" }, storePath);
        if (!okCommit || !string.IsNullOrEmpty(errorCommit))
            return new ResultStruct<byte, Error?>(new GitCommitFailedError());

        return new ResultStruct<byte, Error?>(0);
    }

    public void Initialize()
    {
    }

    #endregion
}