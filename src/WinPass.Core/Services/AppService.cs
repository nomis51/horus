using Spectre.Console;
using WinPass.Core.WinApi;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors;
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

    #endregion

    #region Constructors

    private AppService()
    {
        _fsService = new FsService();
        _gpgService = new GpgService();
    }

    #endregion

    #region Public methods

    public ResultStruct<byte, Error?> RenamePassword(string name, string newName, bool duplicate = false)
    {
        return _fsService.RenameEntry(name, newName, duplicate);
    }

    public ResultStruct<byte, Error?> DeletePassword(string name)
    {
        return _fsService.DeleteEntry(name);
    }

    public Result<string, Error?> GeneratePassword(string name, int length, string customAlphabet, bool copy)
    {
        var value = PasswordHelper.Generate(length, customAlphabet);
        var (_, error) = InsertPassword(name, value);
        if (error is not null) return new Result<string, Error?>(error);

        if (!copy) return new Result<string, Error?>(value);
        User32.SetClipboard(value);

        ProcessHelper.Fork(new[] { "cc", "10" });
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

        var filePath = _fsService.GetPath(name);
        if (_fsService.DoEntryExists(filePath))
            return new ResultStruct<byte, Error?>(new FsPasswordFileAlreadyExistsError());

        var result = _gpgService.Encrypt(gpgKeyId, filePath, value);
        return !_fsService.DoEntryExists(filePath)
            ? new ResultStruct<byte, Error?>(new GpgEncryptError("Resulting entry not found"))
            : result;
    }

    public Result<Password?, Error?> GetPassword(string name, bool copy = false)
    {
        var filePath = _fsService.GetPath(name);
        if (!_fsService.DoEntryExists(filePath)) return new Result<Password?, Error?>(new FsEntryNotFoundError());

        var result = _gpgService.Decrypt(filePath);
        if (!copy) return result;
        if (result.Item1 is null) return result;

        User32.SetClipboard(result.Item1.Value);

        ProcessHelper.Fork(new[] { "cc", "10" });
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
}