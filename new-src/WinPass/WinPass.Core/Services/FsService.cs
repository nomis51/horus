using WinPass.Core.Abstractions;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Core.Services;

public class FsService : IService
{
    #region Constants

    private const string StoreFolderName = ".winpass";
    public const string GpgIdFileName = ".gpg-id";
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

    public Result<string, Error?> GetStoreId()
    {
        var path = Path.Join(GetStoreLocation(), GpgIdFileName);
        return !File.Exists(path)
            ? new Result<string, Error?>(new FsGpgIdKeyNotFoundError())
            : new Result<string, Error?>(File.ReadAllText(path));
    }

    public void Initialize()
    {
        throw new NotImplementedException();
    }

    #endregion
}