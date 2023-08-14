using WinPass.Core.Services.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Display;
using WinPass.Shared.Models.Errors.Git;
using WinPass.Shared.Models.Errors.Gpg;

namespace WinPass.Core.Services;

public class AppService : IAppService
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

    private IFsService _fsService;
    private IGpgService _gpgService;
    private IGitService _gitService;
    private ISettingsService _settingsService;

    #endregion

    #region Constructors

    private AppService()
    {
    }

    #endregion

    #region Public methods

    public EmptyResult RestartGpgAgent()
    {
        return _gpgService.RestartGpgAgent();
    }
    
    public EmptyResult DisableGpgPassPhraseCaching()
    {
        return _fsService.DisablePassPhraseCaching();
    }
    
    public EmptyResult ExportStore(string savePath)
    {
        return _fsService.ExportStore(savePath);
    }
    
    public EmptyResult MigrateStore(string gpgId)
    {
        return _fsService.MigrateStore(gpgId);
    }

    public Result<List<StoreEntry>, Error?> SearchStoreEntries(string text)
    {
        return _fsService.SearchStoreEntries(text);
    }

    public EmptyResult DestroyStore()
    {
        return _fsService.DestroyStore();
    }

    public Result<string, Error?> GitGetRemoteRepositoryName()
    {
        return _gitService.GetRemoteRepositoryName();
    }

    public EmptyResult GitPush()
    {
        return _gitService.Push();
    }

    public ResultStruct<bool, Error?> GitIsAheadOfRemote()
    {
        return _gitService.IsAheadOfRemote();
    }

    public string ExecuteGitCommand(string[] args)
    {
        return _gitService.Execute(args);
    }

    public EmptyResult RenameStoreEntry(string name, string newName, bool duplicate = false)
    {
        return _fsService.RenameStoreEntry(name, newName, duplicate);
    }

    public EmptyResult DeleteStoreEntry(string name)
    {
        return _fsService.RemoveStoreEntry(name);
    }

    public EmptyResult GenerateNewPassword(string name, int length = 0, string customAlphabet = "")
    {
        return _fsService.GenerateNewPassword(name, length, customAlphabet);
    }

    public EmptyResult EditPassword(string name, Password password)
    {
        return _fsService.EditStoreEntryPassword(name, password);
    }

    public EmptyResult EditMetadatas(string name, MetadataCollection metadatas)
    {
        return _fsService.EditStoreEntryMetadatas(name, metadatas);
    }

    public EmptyResult InsertPassword(string name, Password password)
    {
        return _fsService.AddStoreEntry(name, password);
    }

    public bool DoStoreEntryExists(string name)
    {
        return _fsService.DoStoreEntryExists(name);
    }

    public Result<List<StoreEntry>, Error?> GetStoreEntries()
    {
        return _fsService.RetrieveStoreEntries();
    }

    public Result<MetadataCollection, Error?> GetMetadatas(string name)
    {
        var (metadatas, error) = _fsService.RetrieveStoreEntryMetadatas(name);
        return error is not null
            ? new Result<MetadataCollection, Error?>(error)
            : new Result<MetadataCollection, Error?>(metadatas!);
    }

    public Result<Password?, Error?> GetPassword(string name, bool copy = true, int timeout = 10)
    {
        var (password, error) = _fsService.RetrieveStoreEntryPassword(name);
        if (error is not null) return new Result<Password?, Error?>(error);

        if (!copy) return new Result<Password?, Error?>(password);

        ClipboardHelper.Copy(password!.ValueAsString);
        password.Dispose();

        ProcessHelper.Fork(new[] { "cc", timeout <= 0 ? "10" : timeout.ToString() });
        return new Result<Password?, Error?>(password);
    }

    public bool IsStoreInitialized()
    {
        return _fsService.IsStoreInitialized();
    }

    public ResultStruct<bool, Error?> IsGpgIdValid(string id)
    {
        return _gpgService.IsIdValid(id);
    }

    public bool GitClone(string url)
    {
        return _gitService.Clone(url);
    }

    public EmptyResult GitCommit(string message)
    {
        return _gitService.Commit(message);
    }

    public EmptyResult InitializeStoreFolder(string gpgId, string gitUrl)
    {
        return _fsService.InitializeStoreFolder(gpgId, gitUrl);
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

    public bool AcquireLock()
    {
        return _fsService.AcquireLock();
    }

    public void ReleaseLock()
    {
        _fsService.ReleaseLock();
    }

    public Result<Settings?, Error?> GetSettings()
    {
        return _settingsService.GetSettings();
    }

    public EmptyResult SaveSettings(Settings settings)
    {
        return _settingsService.SaveSettings(settings);
    }

    public Result<List<MetadataCollection?>, Error?> DecryptManyMetadatas(List<Tuple<string, string>> items)
    {
        return _gpgService.DecryptManyMetadatas(items);
    }

    public Result<Password?, Error?> DecryptPassword(string path)
    {
        return _gpgService.DecryptPassword(path);
    }

    public Result<MetadataCollection?, Error?> DecryptMetadatas(string path)
    {
        return _gpgService.DecryptMetadatas(path);
    }

    public EmptyResult EncryptPassword(string path, Password password, string gpgId = "")
    {
        return _gpgService.EncryptPassword(path, password, gpgId);
    }

    public EmptyResult EncryptMetadatas(string path, MetadataCollection metadatas, string gpgId = "")
    {
        return _gpgService.EncryptMetadatas(path, metadatas, gpgId);
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

    public EmptyResult Encrypt(string path, string value, string gpgId = "")
    {
        return _gpgService.Encrypt(path, value, gpgId);
    }

    public string GetStoreLocation()
    {
        return _fsService.GetStoreLocation();
    }

    public Result<string, Error?> GetStoreId()
    {
        return _fsService.GetStoreId();
    }

    public void Initialize(AppServiceDependenciesProvider dependencies)
    {
        _fsService = dependencies.FsService;
        _gitService = dependencies.GitService;
        _gpgService = dependencies.GpgService;
        _settingsService = dependencies.SettingsService;

        _fsService.Initialize();
        _gitService.Initialize();
        _gpgService.Initialize();
        _settingsService.Initialize();
    }

    #endregion
}