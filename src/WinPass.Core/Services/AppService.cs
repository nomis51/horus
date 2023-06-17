using WinPass.Core.WinApi;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Fs;
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

    public string GetStorePath()
    {
        return _fsService.GetStorePath();
    }
    
    public void ExecuteGitCommand(string[] args)
    {
        _gitService.Execute(args);
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

    public Result<string, Error?> GeneratePassword(string name, int length, string customAlphabet, bool copy,
        int timeout)
    {
        var value = PasswordHelper.Generate(length, customAlphabet);
        var (_, error) = InsertPassword(name, value);
        if (error is not null) return new Result<string, Error?>(error);

        if (!copy) return new Result<string, Error?>(value);
        User32.SetClipboard(value);

        ProcessHelper.Fork(new[] { "cc", timeout.ToString() });
        return new Result<string, Error?>(value);
    }

    public List<StoreEntry> Search(string term)
    {
        return _fsService.SearchFiles(term);
    }

    public bool DoEntryExists(string name)
    {
        var filePath = _fsService.GetPath(name);
        return _fsService.DoEntryExists(filePath);
    }

    public ResultStruct<byte, Error?> InsertPassword(string name, string value)
    {
        var gpgKeyId = _fsService.GetGpgId();
        if (string.IsNullOrEmpty(gpgKeyId)) return new ResultStruct<byte, Error?>(new FsGpgIdKeyNotFoundError());

        if (_fsService.DoEntryExists(name))
            return new ResultStruct<byte, Error?>(new FsPasswordFileAlreadyExistsError());

        var filePath = _fsService.GetPath(name);
        var (_, errorEncrypt) = _gpgService.Encrypt(gpgKeyId, filePath, value);
        if (!_fsService.DoEntryExists(name))
            return new ResultStruct<byte, Error?>(new GpgEncryptError("Resulting entry not found"));

        if (errorEncrypt is not null) return new ResultStruct<byte, Error?>(errorEncrypt);

        var (_, errorGit) = GitCommit($"Insert password '{name}'");
        return errorGit is not null ? new ResultStruct<byte, Error?>(errorGit) : new ResultStruct<byte, Error?>(0);
    }

    public Result<Password?, Error?> GetPassword(string name, bool copy = false, int timeout = 10)
    {
        if (!_fsService.DoEntryExists(name)) return new Result<Password?, Error?>(new FsEntryNotFoundError());

        var filePath = _fsService.GetPath(name);
        var result = _gpgService.Decrypt(filePath);
        if (!copy) return result;
        if (result.Item1 is null) return result;

        User32.SetClipboard(result.Item1.Value);

        ProcessHelper.Fork(new[] { "cc", timeout.ToString() });
        return result;
    }

    public Result<List<StoreEntry>?, Error?> ListStoreEntries()
    {
        return _fsService.ListStoreEntries();
    }

    public ResultStruct<byte, Error?> InitializeStoreFolder(string gpgKey)
    {
        return _fsService.InitializeStoreFolder(gpgKey);
    }

    public bool DoGpgKeyExists(string key)
    {
        return _gpgService.DoKeyExists(key);
    }

    public void Initialize()
    {
        _fsService.Initialize();
    }

    #endregion

    #region Private methods

    private ResultStruct<byte, Error?> GitCommit(string message)
    {
        return _gitService.Commit(message);
    }

    #endregion
}