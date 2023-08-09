using System.Management.Automation;
using System.Runtime.InteropServices;
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

    public Result<string, Error?> GetRemoteRepositoryName()
    {
        var gitFolder = Path.Join(AppService.Instance.GetStorePath(), ".git");
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

    public ResultStruct<byte, Error?> Push()
    {
        try
        {
            GetPowershellInstance()
                .AddArgument("push")
                .Invoke<string>();
            return new ResultStruct<byte, Error?>(0);
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git push: {Message}", e.Message);
            return new ResultStruct<byte, Error?>(new GitPushFailedError(e.Message));
        }
    }

    public ResultStruct<bool, Error?> IsAheadOfRemote()
    {
        try
        {
            var output = string.Join(
                "\n",
                GetPowershellInstance()
                    .AddArgument("fetch")
                    .AddStatement()
                    .AddCommand(Git)
                    .AddArgument("status")
                    .Invoke<string>()
            );
            return new ResultStruct<bool, Error?>(output.Contains("Your branch is ahead of 'origin/master'"));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git fetch or git status: {Message}", e.Message);
            return new ResultStruct<bool, Error?>(new GitFetchFailedError(e.Message));
        }
    }

    public void Ignore(string filePath)
    {
        var path = AppService.Instance.GetStorePath();
        var gitignoreFilePath = Path.Join(path, ".gitignore");
        var ignorePath = filePath.Replace(path, string.Empty);
        if (!File.Exists(gitignoreFilePath))
        {
            File.WriteAllText(gitignoreFilePath, ignorePath);
            Commit("Add .gitignore");
        }
        else
        {
            var data = File.ReadAllText(gitignoreFilePath);
            data += $"{Environment.NewLine}{ignorePath}";
            File.WriteAllText(gitignoreFilePath, data);
            Commit("Add .gitignore");
        }
    }

    public void DeleteRepository()
    {
        var directory = new DirectoryInfo(AppService.Instance.GetStorePath()) { Attributes = FileAttributes.Normal };

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
            var output = string.Join(
                "\n",
                GetPowershellInstance()
                    .AddArgument("--version")
                    .Invoke<string>()
            );
            return output.StartsWith("git version");
        }
        catch (Exception e)
        {
            Log.Warning("Unable to verify git installation: {Message}", e.Message);
            return false;
        }
    }

    public bool Clone(string url)
    {
        var tmpPath = string.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Directory.Move() fails on linux, because it doesn't allow moving files from different devices (aka /tmp -> /home)
            tmpPath = Path.Join(Environment
                .GetFolderPath(Environment.SpecialFolder.UserProfile), ".tmp");
            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            tmpPath = Path.GetTempPath();
        }

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

        var path = AppService.Instance.GetStorePath();
        Directory.Move(dirPath, path);
        if (!Directory.Exists(Path.Join(path, ".git"))) return false;

        var gpgIdFilePath = Path.Join(path, ".gpg-id");
        if (File.Exists(gpgIdFilePath)) File.Delete(gpgIdFilePath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Directory.Exists(tmpPath)) Directory.Delete(tmpPath);

        return true;
    }

    public string Execute(string[] args)
    {
        var pwsh = GetPowershellInstance();
        foreach (var arg in args)
        {
            pwsh.AddArgument(arg);
        }

        var output = string.Empty;
        try
        {
            output = string.Join("\n", pwsh.Invoke<string>());
        }
        catch (Exception)
        {
            // ignored
        }

        return output;
    }

    public ResultStruct<byte, Error?> Commit(string message)
    {
        try
        {
            var outputAdd = string.Join(
                "\n",
                GetPowershellInstance()
                    .AddArgument("add")
                    .AddArgument("--all")
                    .AddArgument("--")
                    .AddArgument(":!.lock")
                    .Invoke<string>()
            );
            if (!outputAdd.StartsWith("The following paths are ignored by one of your .gitignore files:"))
                return new ResultStruct<byte, Error?>(new GitAddFailedError());
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git commit: {Message}", e.Message);
            return new ResultStruct<byte, Error?>(new GitAddFailedError());
        }


        try
        {
            GetPowershellInstance()
                .AddArgument("commit")
                .AddArgument("-m")
                .AddArgument($"\"{message}\"")
                .Invoke();
        }
        catch (Exception)
        {
            return new ResultStruct<byte, Error?>(new GitCommitFailedError());
        }

        return new ResultStruct<byte, Error?>(0);
    }

    public void Initialize()
    {
    }

    #endregion
    
    #region Private methods

    private PowerShell GetPowershellInstance()
    {
        var pwsh = PowerShell.Create();
        pwsh.Runspace.SessionStateProxy.Path.SetLocation(AppService.Instance.GetStorePath());
        pwsh.AddCommand(Git);
        return pwsh;
    }

    #endregion
}