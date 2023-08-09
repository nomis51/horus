using WinPass.Core.Abstractions;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;

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
    private readonly SettingsService _settingsService;

    #endregion

    #region Constructors

    private AppService()
    {
        _fsService = new FsService();
        _gpgService = new GpgService();
        _gitService = new GitService();
        _settingsService = new SettingsService();
    }

    #endregion

    #region Public methods

    public EmptyResult EncryptPassword(string path, Password password)
    {
        return _gpgService.EncryptPassword(path, password);
    }

    public EmptyResult EncryptMetadatas(string path, MetadataCollection metadatas)
    {
        return _gpgService.EncryptMetadatas(path, metadatas);
    }

    public void GitDeleteRepository()
    {
        _gitService.DeleteRepository();
    }

    public EmptyResult GitIgnore(string filePath)
    {
        return _gitService.Ignore(filePath);
    }

    public Result<string, Error?> Decrypt(string path)
    {
        return _gpgService.Decrypt(path);
    }

    public EmptyResult Encrypt(string path, string value)
    {
        return _gpgService.Encrypt(path, value);
    }

    public string GetStoreLocation()
    {
        return _fsService.GetStoreLocation();
    }

    public Result<string, Error?> GetStoreId()
    {
        return _fsService.GetStoreId();
    }

    public void Initialize()
    {
    }

    #endregion
}