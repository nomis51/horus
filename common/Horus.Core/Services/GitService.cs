using System.Runtime.InteropServices;
using Horus.Core.Services.Abstractions;
using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Errors.Git;
using Horus.Shared.Models.Terminal;
using Serilog;

namespace Horus.Core.Services;

public class GitService : IGitService
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
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "push"
                })
                .Execute();
            return new EmptyResult();
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git push: {Message}", e.Message);
            return new EmptyResult(new GitPushFailedError(e.Message));
        }
    }

    public EmptyResult Pull()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "pull"
                })
                .Execute();
            return new EmptyResult();
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git pull: {Message}", e.Message);
            return new EmptyResult(new GitPushFailedError(e.Message));
        }
    }

    public Result<Tuple<int, int>, Error?> Fetch()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "fetch"
                })
                .Command(new[]
                {
                    GitProcessName,
                    "status"
                })
                .Execute();

            var aheadBy = 0;
            var behindBy = 0;

            const string aheadLabel = "Your branch is ahead of '";
            var aheadLine = result.OutputLines.FirstOrDefault(l => l.Contains(aheadLabel));
            if (!string.IsNullOrEmpty(aheadLine))
            {
                var index = aheadLine.IndexOf(aheadLabel, StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    const string byLabel = "' by ";
                    var endIndex = aheadLine.IndexOf(byLabel, index, StringComparison.OrdinalIgnoreCase);
                    if (endIndex != -1)
                    {
                        endIndex += byLabel.Length;
                        var nextIndex = aheadLine.IndexOf(" commit", endIndex, StringComparison.OrdinalIgnoreCase);
                        var value = aheadLine[endIndex..nextIndex];
                        if (int.TryParse(value, out var intValue))
                        {
                            aheadBy = intValue;
                        }
                    }
                }
            }

            const string behindByLabel = "Your branch is behind of '";
            var behindLine = result.OutputLines.FirstOrDefault(l => l.Contains(behindByLabel));
            if (!string.IsNullOrEmpty(behindLine))
            {
                var index = behindLine.IndexOf(behindByLabel, StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    const string byLabel = "' by ";
                    var endIndex = behindLine.IndexOf(byLabel, index, StringComparison.OrdinalIgnoreCase);
                    if (endIndex != -1)
                    {
                        endIndex += byLabel.Length;
                        var nextIndex = behindLine.IndexOf(" commit", endIndex, StringComparison.OrdinalIgnoreCase);
                        var value = behindLine[endIndex..nextIndex];
                        if (int.TryParse(value, out var intValue))
                        {
                            behindBy = intValue;
                        }
                    }
                }
            }

            return new Result<Tuple<int, int>, Error?>(Tuple.Create(aheadBy, behindBy));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git fetch or git status: {Message}", e.Message);
            return new Result<Tuple<int, int>, Error?>(new GitFetchFailedError(e.Message));
        }
    }

    public ResultStruct<bool, Error?> IsAheadOfRemote()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "fetch"
                })
                .Command(new[]
                {
                    GitProcessName,
                    "status"
                })
                .Execute();

            return new ResultStruct<bool, Error?>(
                result.OutputLines.FirstOrDefault()?.Contains("Your branch is ahead of 'origin/master'") ?? true);
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
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "--version"
                })
                .Execute();
            return result.OutputLines.FirstOrDefault()?.StartsWith("git version") ?? false;
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
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "clone",
                    url,
                    dirPath
                })
                .Execute();

            // For some reason, git clone output to stderr even thought there is no error
            if (!result.ErrorLines.FirstOrDefault()?.Contains("Cloning into") ?? true)
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
        if (Directory.Exists(storeLocation)) DeleteRepository();

        Directory.Move(dirPath, storeLocation);
        if (!Directory.Exists(Path.Join(storeLocation, ".git"))) return false;

        var gpgIdFilePath = Path.Join(storeLocation, FsService.GpgIdFileName);
        if (File.Exists(gpgIdFilePath)) File.Delete(gpgIdFilePath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Directory.Exists(tmpPath)) Directory.Delete(tmpPath);

        return true;
    }

    public string Execute(string[] args)
    {
        var output = string.Empty;
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(args)
                .Execute();
            output = string.Join("\n", result.OutputLines);
            if (result.ErrorLines.Count != 0)
            {
                output += $"\n\n{string.Join("\n", result.ErrorLines)}";
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
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "add",
                    "--all",
                    "--",
                    ":!.lock"
                })
                .Execute();
            if (!result.Successful && result.ErrorLines.Count != 0 &
                result.ErrorLines.FirstOrDefault(l => l.Contains("The following paths are ignored by one of your .gitignore files:")) is null)
                throw new Exception(string.Join("\n", result.ErrorLines));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git add: {Message}", e.Message);
            return new EmptyResult(new GitAddFailedError());
        }

        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "commit",
                    "-m",
                    $"\"{message}\"",
                })
                .Execute();
            if (!result.Successful && result.OutputLines.FirstOrDefault(l => l.Contains("nothing to commit, working tree clean")) is null)
                throw new Exception(string.Join("\n", result.ErrorLines));
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

    public EmptyResult CreateBranch(string name)
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "branch",
                    name
                })
                .Execute();
            if (!result.Successful) throw new Exception(string.Join("\n", result.ErrorLines));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git branch '{Branch}': {Message}", name, e.Message);
            return new EmptyResult(new GitCommitFailedError());
        }

        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "push",
                    "-u",
                    "origin",
                    name
                })
                .Execute();
            if (!result.Successful) throw new Exception(string.Join("\n", result.ErrorLines));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git push -u origin '{Branch}': {Message}", name, e.Message);
            return new EmptyResult(new GitCommitFailedError());
        }

        return new EmptyResult();
    }

    public EmptyResult ChangeBranch(string name)
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "checkout",
                    name
                })
                .Execute();
            if (!result.Successful) throw new Exception(string.Join("\n", result.ErrorLines));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git checkout '{Branch}': {Message}", name, e.Message);
            return new EmptyResult(new GitCommitFailedError());
        }

        return new EmptyResult();
    }

    public EmptyResult RemoveBranch(string name)
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "branch",
                    "-D",
                    name
                })
                .Execute();
            if (!result.Successful) throw new Exception(string.Join("\n", result.ErrorLines));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git branch -D '{Branch}': {Message}", name, e.Message);
            return new EmptyResult(new GitCommitFailedError());
        }

        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "push",
                    "origin",
                    "-d",
                    name
                })
                .Execute();
            if (!result.Successful) throw new Exception(string.Join("\n", result.ErrorLines));
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git push origin -d '{Branch}': {Message}", name, e.Message);
            return new EmptyResult(new GitCommitFailedError());
        }

        return new EmptyResult();
    }

    public Result<List<string>, Error?> ListBranches()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "branch",
                })
                .Execute();
            if (!result.Successful) throw new Exception(string.Join("\n", result.ErrorLines));

            return new Result<List<string>, Error?>(
                result.OutputLines
                    .SelectMany(l => l.Split("\n"))
                    .Select(l => l.Replace("*", string.Empty).Trim())
                    .ToList()
            );
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git branch: {Message}", e.Message);
            return new Result<List<string>, Error?>(new GitBranchError(e.Message));
        }
    }

    public Result<string, Error?> GetCurrentBranch()
    {
        try
        {
            var result = new TerminalSession(AppService.Instance.GetStoreLocation())
                .Command(new[]
                {
                    GitProcessName,
                    "branch",
                })
                .Execute();
            if (!result.Successful) throw new Exception(string.Join("\n", result.ErrorLines));

            return new Result<string, Error?>(
                (
                    result.OutputLines
                        .SelectMany(l => l.Split("\n"))
                        .FirstOrDefault(l => l.StartsWith("*", StringComparison.OrdinalIgnoreCase)) ?? string.Empty
                )
                .Trim('*')
                .Trim()
            );
        }
        catch (Exception e)
        {
            Log.Error("Error while performing git branch: {Message}", e.Message);
            return new Result<string, Error?>(new GitCurrentBranchError(e.Message));
        }
    }

    public void Initialize()
    {
    }

    #endregion
}