using System.Management.Automation;
using System.Runtime.InteropServices;
using Serilog;
using WinPass.Core.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Git;

namespace WinPass.Core.Services;

public class GitService : IService
{
    #region Constants

    private const string GitProcessName = "git";

    #endregion

    #region Public methods

    public Result<string, Error?> GetRemoteRepositoryName()
    {
        var gitFolder = Path.Join(AppService.Instance.GetStoreLocation(), ".git");
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

    public EmptyResult Push()
    {
        try
        {
            GetPowerShellInstance()
                .AddArgument("push")
                .Invoke<string>();
            return new EmptyResult();
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git push: {Message}", e.Message);
            return new EmptyResult(new GitPushFailedError(e.Message));
        }
    }

    public ResultStruct<bool, Error?> IsAheadOfRemote()
    {
        try
        {
            var lines = GetPowerShellInstance()
                .AddArgument("fetch")
                .AddStatement()
                .AddCommand(GitProcessName)
                .AddArgument("status")
                .Invoke<string>();
            return new ResultStruct<bool, Error?>(
                lines.FirstOrDefault()?.Contains("Your branch is ahead of 'origin/master'") ?? true);
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git fetch or git status: {Message}", e.Message);
            return new ResultStruct<bool, Error?>(new GitFetchFailedError(e.Message));
        }
    }

    public void DeleteRepository()
    {
        var path = AppService.Instance.GetStoreLocation();
        if (!Directory.Exists(path)) return;

        var directory = new DirectoryInfo(path)
            { Attributes = FileAttributes.Normal };

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
            var lines = GetPowerShellInstance()
                .AddArgument("--version")
                .Invoke<string>();
            return lines.FirstOrDefault()?.StartsWith("git version") ?? false;
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

        var dirName = Guid.NewGuid().ToString();
        if (string.IsNullOrEmpty(dirName)) return false;

        var dirPath = Path.Join(tmpPath, dirName);
        if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);

        try
        {
            var pwsh = GetPowerShellInstance()
                .AddArgument("clone")
                .AddArgument(url)
                .AddArgument($"{dirPath}");
            pwsh.Invoke<string>();
            // For some reason, git clone output to stderr even thought there is no error
            var lines = pwsh.Streams.Error.ReadAll().Select(e => e.Exception.Message);

            if (!lines.FirstOrDefault()?.Contains("Cloning into") ?? true)
            {
                if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);
            }
        }
        catch (Exception e)
        {
            Log.Warning("Unable to verify git installation: {Message}", e.Message);
            if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);
        }

        if (!Directory.Exists(dirPath)) return false;

        var storeLocation = AppService.Instance.GetStoreLocation();
        if (Directory.Exists(storeLocation)) Directory.Delete(storeLocation);

        Directory.Move(dirPath, storeLocation);
        if (!Directory.Exists(Path.Join(storeLocation, ".git"))) return false;

        var gpgIdFilePath = Path.Join(storeLocation, FsService.GpgIdFileName);
        if (File.Exists(gpgIdFilePath)) File.Delete(gpgIdFilePath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Directory.Exists(tmpPath)) Directory.Delete(tmpPath);

        return true;
    }

    public string Execute(string[] args)
    {
        var pwsh = GetPowerShellInstance();
        foreach (var arg in args)
        {
            pwsh.AddArgument(arg);
        }

        var output = string.Empty;
        try
        {
            output = string.Join("\n", pwsh.Invoke<string>());
            if (pwsh.HadErrors)
            {
                output += $"\n\n{string.Join("\n", pwsh.Streams.Error.ReadAll())}";
            }
        }
        catch (Exception e)
        {
            Log.Error("Error while performing user git command: {Message}", e.Message);
        }

        return output;
    }

    public EmptyResult Commit(string message)
    {
        try
        {
            var lines = GetPowerShellInstance()
                .AddArgument("add")
                .AddArgument("--all")
                .AddArgument("--")
                .AddArgument(":!.lock")
                .Invoke<string>();
            if (lines.FirstOrDefault()
                    ?.StartsWith("The following paths are ignored by one of your .gitignore files:") ?? true)
                return new EmptyResult(new GitAddFailedError());
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git add: {Message}", e.Message);
            return new EmptyResult(new GitAddFailedError());
        }

        try
        {
            // TODO: catch from output if it failed
            GetPowerShellInstance()
                .AddArgument("commit")
                .AddArgument("-m")
                .AddArgument($"\"{message}\"")
                .Invoke();
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git commit: {Message}", e.Message);
            return new EmptyResult(new GitCommitFailedError());
        }

        return new EmptyResult();
    }

    public EmptyResult Ignore(string filePath)
    {
        var storeLocation = AppService.Instance.GetStoreLocation();
        var gitignoreFilePath = Path.Join(storeLocation, ".gitignore");
        var ignorePath = filePath.Replace(storeLocation, string.Empty);

        if (!File.Exists(gitignoreFilePath))
        {
            File.WriteAllText(gitignoreFilePath, ignorePath);
            var error = Commit("Add .gitignore");
            if (error.HasError) return new EmptyResult(error.Error!);
        }
        else
        {
            var data = File.ReadAllText(gitignoreFilePath);
            data += $"{Environment.NewLine}{ignorePath}";
            File.WriteAllText(gitignoreFilePath, data);
            var error = Commit("Add .gitignore");
            if (error.HasError) return new EmptyResult(error.Error!);
        }

        return new EmptyResult();
    }

    public void Initialize()
    {
    }

    #endregion

    #region Private methods

    private PowerShell GetPowerShellInstance()
    {
        var pwsh = PowerShell.Create();
        pwsh.Runspace.SessionStateProxy.Path.SetLocation(AppService.Instance.GetStoreLocation());
        pwsh.AddCommand(GitProcessName);
        return pwsh;
    }

    #endregion
}