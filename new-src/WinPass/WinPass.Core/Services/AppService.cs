using WinPass.Core.Abstractions;

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

    public string GetStoreLocation()
    {
        return _fsService.GetStoreLocation();
    }
    
    public void Initialize()
    {
    }

    #endregion
}