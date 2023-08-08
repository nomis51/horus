using WinPass.Core.Abstractions;

namespace WinPass.Core.Services;

public class FsService : IService
{
    #region Constants

    private const string StoreFolderName = ".winpass";
    public const string GigIdFileName = ".gpg-id";
    private const string AppLockFileName = ".lock";

    private readonly string _storeFolderPathTemplate =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{StoreFolderName}/";

    private readonly string _storeFolderPath;

    #endregion

    #region Members

    private FileStream? _lockFileStream;

    #endregion

    #region Constructors

    public FsService()
    {
        _storeFolderPath = Environment.ExpandEnvironmentVariables(_storeFolderPathTemplate);
    }

    #endregion

    #region Public methods

    public string GetStoreLocation()
    {
        return _storeFolderPath;
    }

    public void Initialize()
    {
        throw new NotImplementedException();
    }

    #endregion
}