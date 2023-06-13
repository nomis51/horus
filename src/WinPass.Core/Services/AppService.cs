using WinPass.Core.WinApi;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors;
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

    public Result<Password?, Error?> GetPassword(string name, bool copy = false)
    {
        var path = _fsService.GetPath(name);
        var result = _gpgService.Decrypt(path);
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