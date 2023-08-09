using Serilog;
using WinPass.Core.Abstractions;
using WinPass.Shared.Enums;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Errors.Gpg;

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

    public EmptyResult AddStoreEntry(string name, Password password)
    {
        if (DoStoreEntryExists(name))
            return new EmptyResult(new FsPasswordFileAlreadyExistsError());

        var filePath = Path.Join(GetStoreLocation(), name);
        if (name.Contains('/') || name.Contains('\\'))
        {
            var dirName = Path.GetDirectoryName(filePath)!;
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        var metadatasFilePath = GetMetadataPath(filePath);
        var resultEncryptMetadatas = AppService.Instance.EncryptMetadatas(metadatasFilePath, new MetadataCollection
        {
            new("created", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"), MetadataType.Internal),
            new("modified", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"), MetadataType.Internal),
        });
        if (resultEncryptMetadatas.HasError) return new EmptyResult(resultEncryptMetadatas.Error!);

        var resultEncryptPassword = AppService.Instance.EncryptPassword(filePath, password);
        if (!resultEncryptPassword.HasError) return new EmptyResult();

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        if (File.Exists(metadatasFilePath))
        {
            File.Delete(metadatasFilePath);
        }

        return new EmptyResult(resultEncryptPassword.Error!);
    }

    public bool DoStoreEntryExists(string name, bool byPath = false)
    {
        return File.Exists(Path.Join(GetStoreLocation(), name));
    }

    public EmptyResult DestroyStore()
    {
        if (!VerifyLock()) return new EmptyResult(new GpgDecryptLockFileError());

        ReleaseLock();
        AppService.Instance.GitDeleteRepository();
        return new EmptyResult();
    }

    public void ReleaseLock()
    {
        if (_lockFileStream is null) return;
        _lockFileStream.Close();
        _lockFileStream = null;
    }

    public bool AcquireLock()
    {
        if (_lockFileStream is not null) return false;

        var filePath = Path.Join(GetStoreLocation(), AppLockFileName);
        if (!File.Exists(filePath))
        {
            var resultEncrypt = AppService.Instance.Encrypt(filePath, AppLockFileName);
            if (resultEncrypt.HasError)
            {
                Log.Error("Unable to encrypt lock file: {Message}", resultEncrypt.Error!.Message);
                return false;
            }

            var resultGitIgnore = AppService.Instance.GitIgnore(filePath);
            if (resultGitIgnore.HasError)
            {
                Log.Error("Unable to git ignore lock file: {Message}", resultGitIgnore.Error!.Message);
                return false;
            }
        }

        try
        {
            _lockFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
        }
        catch (Exception e)
        {
            Log.Error("Unable to acquire lock: {Messagse}", e.Message);
            return false;
        }

        return true;
    }

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
    }

    #endregion

    #region Private methods

    private string GetMetadataPath(string path)
    {
        return path.Insert(path.LastIndexOf(".gpg", StringComparison.OrdinalIgnoreCase), ".m");
    }

    private bool VerifyLock()
    {
        if (_lockFileStream is null) return false;

        var filePath = Path.Join(_storeFolderPath, AppLockFileName);
        if (!File.Exists(filePath)) return false;

        _lockFileStream.Position = 0;
        using var ms = new MemoryStream();
        _lockFileStream.CopyTo(ms);
        var tmpFile = Path.GetTempFileName();
        File.WriteAllBytes(tmpFile, ms.ToArray());

        var (content, error) = AppService.Instance.Decrypt(tmpFile);
        File.Delete(tmpFile);

        return error is null && content == AppLockFileName;
    }

    #endregion
}