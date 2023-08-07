using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Errors.Git;
using WinPass.Shared.Models.Errors.Gpg;
using WinPass.Shared.Models.Fs;

namespace WinPass.Core.Services;

public class AppService : IService
{
    #region Singleton

    private static readonly object LockInstance = new();
#pragma warning disable CS8618
    private static AppService _instance;
#pragma warning restore CS8618

    public static AppService Instance
    {
        get
        {
            lock (LockInstance)
            {
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                _instance ??= new AppService();
            }

            return _instance;
        }
    }

    #endregion

    #region Services

    private readonly FsService _fsService;
    private readonly GpgService _gpgService;
    private readonly GitService _gitService;

    #endregion

    #region Constructors

    private AppService()
    {
        _fsService = new FsService();
        _gpgService = new GpgService();
        _gitService = new GitService();
    }

    #endregion

    #region Public methods

    public Result<string, Error?> GetRemoteRepositoryName()
    {
        return _gitService.GetRemoteRepositoryName();
    }

    public ResultStruct<byte, Error?> GitPush()
    {
        return _gitService.Push();
    }

    public ResultStruct<bool, Error?> IsAheadOfRemote()
    {
        return _gitService.IsAheadOfRemote();
    }

    public bool AcquireLock()
    {
        return _fsService.AcquireLock();
    }

    public void ReleaseLock()
    {
        _fsService.ReleaseLock();
    }

    public void DeleteRepository( )
    {
        _gitService.DeleteRepository();
    }

    public ResultStruct<byte, Error?> DestroyStore()
    {
        return _fsService.DestroyStore();
    }

    public bool IsStoreInitialized()
    {
        return _fsService.IsStoreInitialized();
    }

    public ResultStruct<byte, Error?> SaveSettings(Settings settings)
    {
        return _fsService.SaveSettings(settings);
    }

    public Result<Settings?, Error?> GetSettings()
    {
        return _fsService.GetSettings();
    }

    public ResultStruct<byte, Error?> Encrypt(Gpg gpg, string filePath, string value)
    {
        return _gpgService.Encrypt(gpg, filePath, value);
    }

    public ResultStruct<byte, Error?> DecryptLock(Gpg gpg, string filePath)
    {
        return _gpgService.DecryptLock(gpg, filePath);
    }

    public ResultStruct<byte, Error?> Verify()
    {
        var gitOk = false;
        var gpgOk = false;
        var tasks = new Task[2];
        tasks[0] = Task.Run(() => gpgOk = _gpgService.Verify());
        tasks[1] = Task.Run(() => gitOk = _gitService.Verify());
        Task.WaitAll(tasks);

        if (!gpgOk) return new ResultStruct<byte, Error?>(new GpgNotInstalledError());
        return !gitOk
            ? new ResultStruct<byte, Error?>(new GitNotInstalledError())
            : new ResultStruct<byte, Error?>(0);
    }

    public string GetStorePath()
    {
        return _fsService.GetStorePath();
    }

    public string ExecuteGitCommand(string[] args)
    {
        return _gitService.Execute(args);
    }

    public ResultStruct<byte, Error?> EditPassword(string name, Password password)
    {
        var (_, error) = _fsService.EditEntry(name, password);
        if (error is not null) return new ResultStruct<byte, Error?>(error);

        var (_, errorGit) = GitCommit($"Edit password '{name}'");
        return errorGit is not null ? new ResultStruct<byte, Error?>(errorGit) : new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<byte, Error?> RenamePassword(string name, string newName, bool duplicate = false)
    {
        var (_, error) = _fsService.RenameEntry(name, newName, duplicate);
        if (error is not null) return new ResultStruct<byte, Error?>(error);

        var (_, errorGit) = GitCommit($"{(duplicate ? "Duplicate" : "Rename")} password '{name}' to '{newName}'");
        return errorGit is not null ? new ResultStruct<byte, Error?>(errorGit) : new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<byte, Error?> DeletePassword(string name)
    {
        var (_, error) = _fsService.DeleteEntry(name);
        if (error is not null) return new ResultStruct<byte, Error?>(error);

        var (_, errorGit) = GitCommit($"Delete password '{name}'");
        return errorGit is not null ? new ResultStruct<byte, Error?>(errorGit) : new ResultStruct<byte, Error?>(0);
    }

    public Result<byte[], Error?> GeneratePassword()
    {
        var (settings, error) = _fsService.GetSettings();
        if (error is not null) return new Result<byte[], Error?>(error);

        return new Result<byte[], Error?>(
            PasswordHelper.Generate(
                settings!.DefaultLength,
                settings.DefaultCustomAlphabet
            )
        );
    }

    public Result<Password?, Error?> GeneratePassword(string name, int length, string customAlphabet, bool copy,
        int timeout)
    {
        var password = new Password
        {
            ValueBytes = PasswordHelper.Generate(length, customAlphabet),
        };
        var (_, error) = InsertPassword(name, password);
        if (error is not null) return new Result<Password?, Error?>(error);

        if (!copy) return new Result<Password?, Error?>(password);
        ClipboardHelper.Copy(password.ValueAsString);

        ProcessHelper.Fork(new[] { "cc", timeout <= 0 ? "10" : timeout.ToString() });
        return new Result<Password?, Error?>(password);
    }

    public List<StoreEntry> Search(string term)
    {
        return _fsService.SearchFiles(term);
    }

    public bool DoEntryExists(string name)
    {
        return _fsService.DoEntryExists(name);
    }

    public ResultStruct<byte, Error?> InsertPassword(string name, Password password, bool dontCommit = false)
    {
        var (_, error) = _fsService.InsertEntry(name, password);
        if (error is not null) return new ResultStruct<byte, Error?>(error);

        if (!dontCommit)
        {
            var (_, errorGit) = GitCommit($"Insert password '{name}'");
            if (errorGit is not null) return new ResultStruct<byte, Error?>(errorGit);
        }

        return new ResultStruct<byte, Error?>(0);
    }

    public Result<Password?, Error?> GetPassword(string name, bool copy = false, int timeout = 10,
        bool onlyMetadata = false)
    {
        if (!_fsService.DoEntryExists(name)) return new Result<Password?, Error?>(new FsEntryNotFoundError());

        var gpgKeyId = _fsService.GetGpgId();
        if (string.IsNullOrEmpty(gpgKeyId)) return new Result<Password?, Error?>(new FsGpgIdKeyNotFoundError());

        var gpg = new Gpg(gpgKeyId);

        var filePath = _fsService.GetPath(name);
        var result = _gpgService.DecryptPassword(gpg, filePath, onlyMetadata);
        if (!copy) return result;
        if (result.Item1 is null) return result;

        ClipboardHelper.Copy(result.Item1.ValueAsString);

        ProcessHelper.Fork(new[] { "cc", timeout <= 0 ? "10" : timeout.ToString() });
        return result;
    }

    public Result<List<StoreEntry>?, Error?> ListStoreEntries()
    {
        return _fsService.ListStoreEntries();
    }

    public ResultStruct<byte, Error?> InitializeStoreFolder(string gpgKey, string gitUrl)
    {
        if (!_gitService.Clone(gitUrl))
            return new ResultStruct<byte, Error?>(new GitCloneFailedError());

        var (_, error) = _fsService.InitializeStoreFolder(gpgKey);
        if (error is not null)
        {
            _gitService.DeleteRepository();
            return new ResultStruct<byte, Error?>(error);
        }

        var (_, errorCommit) = _gitService.Commit("Add '.gpg-id' file");
        if (errorCommit is not null)
        {
            _gitService.DeleteRepository();
            return new ResultStruct<byte, Error?>(errorCommit);
        }

        return new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<byte, Error?> GitCommit(string message)
    {
        return _gitService.Commit(message);
    }

    public ResultStruct<bool, Error?> IsKeyValid(Gpg gpg)
    {
        return _gpgService.IsKeyValid(gpg);
    }

    public void GitIgnore(string filePath)
    {
        _gitService.Ignore(filePath);
    }

    public void Initialize()
    {
        _fsService.Initialize();
    }

    #endregion
}