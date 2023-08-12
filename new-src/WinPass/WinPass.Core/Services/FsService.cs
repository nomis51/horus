using Serilog;
using WinPass.Core.Abstractions;
using WinPass.Shared.Enums;
using WinPass.Shared.Extensions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Display;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Errors.Git;
using WinPass.Shared.Models.Errors.Gpg;

namespace WinPass.Core.Services;

public class FsService : IService
{
    #region Constants

    private const string StoreFolderName = ".winpass";
    private const string MigrationStoreFolderName = $"{StoreFolderName}-migration";
    public const string GpgIdFileName = ".gpg-id";
    private const string AppLockFileName = ".lock";

    private readonly string _storeFolderPathTemplate =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{StoreFolderName}/";

    private readonly string _migrationStoreFolderPathTemplate =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{MigrationStoreFolderName}/";

    private readonly string _storeFolderPath;
    private readonly string _migrationStoreFolderPath;

    #endregion

    #region Members

    private FileStream? _lockFileStream;

    #endregion

    #region Constructors

    public FsService()
    {
        _storeFolderPath = Environment.ExpandEnvironmentVariables(_storeFolderPathTemplate);
        _migrationStoreFolderPath = Environment.ExpandEnvironmentVariables(_migrationStoreFolderPathTemplate);
    }

    #endregion

    #region Public methods

    public EmptyResult MigrateStore(string gpgId)
    {
        if (!VerifyLock()) return new EmptyResult(new GpgDecryptError("Lock check failed"));

        List<Tuple<string, string>> storeFilePaths = new();
        EnumerateFilePaths(GetStoreLocation(), storeFilePaths);

        if (Directory.Exists(_migrationStoreFolderPath)) Directory.Delete(_migrationStoreFolderPath, true);
        Directory.CreateDirectory(_migrationStoreFolderPath);

        foreach (var (_, filePath) in storeFilePaths)
        {
            if (filePath.EndsWith(".m.gpg")) continue;

            var metadataFilePath = filePath.Replace(".gpg", ".m.gpg");
            var newMetadataFilePath = metadataFilePath.Replace(StoreFolderName, MigrationStoreFolderName);
            var (metadatas, errorDecryptMetadatas) = AppService.Instance.DecryptMetadatas(metadataFilePath);
            if (errorDecryptMetadatas is not null)
            {
                Directory.Delete(_migrationStoreFolderPath);
                return new EmptyResult(new GpgDecryptError(errorDecryptMetadatas.Message));
            }

            var encryptMetadatasResult = AppService.Instance.EncryptMetadatas(newMetadataFilePath, metadatas!, gpgId);
            if (encryptMetadatasResult.HasError)
            {
                Directory.Delete(_migrationStoreFolderPath);
                return new EmptyResult(new GpgDecryptError(encryptMetadatasResult.Error!.Message));
            }

            var newFilePath = filePath.Replace(StoreFolderName, MigrationStoreFolderName);
            var (password, errorDecryptPassword) = AppService.Instance.DecryptPassword(filePath);
            if (errorDecryptPassword is not null)
            {
                Directory.Delete(_migrationStoreFolderPath);
                return new EmptyResult(new GpgDecryptError(errorDecryptPassword.Message));
            }

            var encryptPasswordResult = AppService.Instance.EncryptPassword(newFilePath, password!, gpgId);
            if (encryptPasswordResult.HasError)
            {
                Directory.Delete(_migrationStoreFolderPath);
                return new EmptyResult(new GpgDecryptError(encryptPasswordResult.Error!.Message));
            }
        }

        var existingFiles = Directory.GetFiles(_storeFolderPath, "*.gpg", SearchOption.AllDirectories);
        foreach (var existingFile in existingFiles)
        {
            File.Delete(existingFile);
        }

        foreach (var newFile in Directory.GetFiles(_migrationStoreFolderPath, "*.gpg", SearchOption.AllDirectories))
        {
            File.Move(newFile, newFile.Replace(MigrationStoreFolderName, StoreFolderName));
        }

        File.WriteAllText(Path.Join(_storeFolderPath, GpgIdFileName), gpgId);

        if (Directory.Exists(_migrationStoreFolderPath)) Directory.Delete(_migrationStoreFolderPath, true);

        var resultGitCommit = AppService.Instance.GitCommit($"Migrate store to new GPG keypair '{gpgId}'");
        if (resultGitCommit.HasError)
        {
            return new EmptyResult(resultGitCommit.Error!);
        }

        return new EmptyResult();
    }

    public EmptyResult GenerateNewPassword(string name, int length = 0, string customAlphabet = "")
    {
        var (settings, error) = AppService.Instance.GetSettings();
        if (error is not null) return new EmptyResult(error);

        var newPassword = new Password(
            name,
            PasswordHelper.Generate(
                length <= 0 ? settings!.DefaultLength : length,
                string.IsNullOrWhiteSpace(customAlphabet) ? settings!.DefaultCustomAlphabet : customAlphabet
            )
        );

        var result = DoStoreEntryExists(name)
            ? EditStoreEntryPassword(name, newPassword)
            : AddStoreEntry(name, newPassword);
        newPassword.Dispose();

        return result;
    }

    public Result<List<StoreEntry>, Error?> SearchStoreEntries(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new Result<List<StoreEntry>, Error?>(Enumerable.Empty<StoreEntry>().ToList());

        List<Tuple<string, string>> items = new();
        var storePath = GetStoreLocation();

        EnumerateFilePaths(storePath, items);

        var (lstMetadatas, error) = AppService.Instance.DecryptManyMetadatas(items);
        if (error is not null) return new Result<List<StoreEntry>, Error?>(error);

        // TODO: search
        var loweredText = text.Trim().ToLower();
        List<StoreEntry> entries = new();
        LocalSearchEntries(storePath, string.Empty);

        return new Result<List<StoreEntry>, Error?>(entries);

        void LocalSearchEntries(string current, string currenPath)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var filePath in Directory.EnumerateFiles(current))
            {
                if (!filePath.EndsWith(".gpg")) continue;

                var name = Path.GetFileName(filePath);
                var currentEntryPath = $"{currenPath}/{name}";
                var metadatas = lstMetadatas.FirstOrDefault(m => m?.Name == currentEntryPath);

                List<string> metadataFound = new();
                if (metadatas is not null)
                {
                    foreach (var metadata in metadatas)
                    {
                        if (!metadata.Key.ToLower().Contains(loweredText) &&
                            !metadata.Value.ToLower().Contains(loweredText)) continue;
                        
                        metadataFound.Add(metadata.ToString());
                    }
                }

                if (!name.ToLower().Contains(loweredText) && !metadataFound.Any()) continue;

                entries.Add(
                    new StoreEntry(
                        name,
                        false,
                        metadataFound.Any(),
                        metadataFound
                    )
                );
            }

            foreach (var dirPath in Directory.EnumerateDirectories(current))
            {
                if (dirPath.EndsWith(".git")) continue;

                var name = Path.GetFileName(dirPath);
                LocalSearchEntries(dirPath, $"{currenPath}/{name}");
            }
        }
    }

    public Result<List<StoreEntry>, Error?> RetrieveStoreEntries()
    {
        List<StoreEntry> results = new();
        var storePath = GetStoreLocation();

        LocalEnumerateEntries(storePath, results);

        return new Result<List<StoreEntry>, Error?>(results);

        void LocalEnumerateEntries(string current, ICollection<StoreEntry> entries)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var filePath in Directory.EnumerateFiles(current))
            {
                if (!filePath.EndsWith(".gpg") || filePath.EndsWith(".m.gpg")) continue;

                entries.Add(
                    new StoreEntry(
                        Path.GetFileName(filePath)
                            .Split(".", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .First()
                    )
                );
            }

            foreach (var dirPath in Directory.EnumerateDirectories(current))
            {
                if (dirPath.EndsWith(".git")) continue;

                entries.Add(
                    new StoreEntry(
                        Path.GetFileName(dirPath),
                        true
                    )
                );

                LocalEnumerateEntries(dirPath, entries.Last().Entries);
            }
        }
    }

    public EmptyResult RemoveStoreEntry(string name)
    {
        if (!DoStoreEntryExists(name)) return new EmptyResult(new FsEntryNotFoundError());

        if (!VerifyLock()) return new EmptyResult(new GpgDecryptError("Lock check failed"));

        var filePath = GetEntryPath(name);
        var metadatasFilePath = GetMetadataPath(name);
        File.Delete(filePath);
        File.Delete(metadatasFilePath);

        var resultGitCommit = AppService.Instance.GitCommit($"Remove password '{name}'");
        return resultGitCommit.HasError ? new EmptyResult(resultGitCommit.Error!) : new EmptyResult();
    }

    public EmptyResult RenameStoreEntry(string name, string newName, bool duplicate = false)
    {
        if (!DoStoreEntryExists(name)) return new EmptyResult(new FsEntryNotFoundError());
        if (DoStoreEntryExists(newName)) return new EmptyResult(new FsPasswordFileAlreadyExistsError());

        var filePath = GetEntryPath(name);
        var metadatasFilePath = GetMetadataPath(name);
        var newFilePath = GetEntryPath(newName);
        var newMetadatasFilePath = GetMetadataPath(newName);

        if (duplicate)
        {
            File.Copy(filePath, newFilePath);
            File.Copy(metadatasFilePath, newMetadatasFilePath);
        }
        else
        {
            File.Move(filePath, newFilePath);
            File.Move(metadatasFilePath, newMetadatasFilePath);
        }

        var resultGitCommit =
            AppService.Instance.GitCommit($"{(duplicate ? "Rename" : "Duplicate")} password '{name}' to '{newName}'");
        return resultGitCommit.HasError ? new EmptyResult(resultGitCommit.Error!) : new EmptyResult();
    }

    public Result<Password?, Error?> RetrieveStoreEntryPassword(string name)
    {
        if (!DoStoreEntryExists(name)) return new Result<Password?, Error?>(new FsEntryNotFoundError());

        var filePath = GetEntryPath(name);

        var (password, error) = AppService.Instance.DecryptPassword(filePath);
        return error is not null
            ? new Result<Password?, Error?>(error)
            : new Result<Password?, Error?>(password);
    }

    public Result<MetadataCollection?, Error?> RetrieveStoreEntryMetadatas(string name)
    {
        if (!DoStoreEntryExists(name, true))
        {
            return new Result<MetadataCollection?, Error?>(new MetadataCollection
            {
                new("created", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"), MetadataType.Internal),
                new("modified", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"), MetadataType.Internal),
            });
        }

        var metadatasFilePath = GetMetadataPath(name);

        var (metadatas, error) = AppService.Instance.DecryptMetadatas(metadatasFilePath);
        return error is not null
            ? new Result<MetadataCollection?, Error?>(error)
            : new Result<MetadataCollection?, Error?>(metadatas);
    }

    public EmptyResult EditStoreEntryMetadatas(string name, MetadataCollection metadatas)
    {
        if (!DoStoreEntryExists(name, true)) return new EmptyResult(new FsEntryNotFoundError());

        var result = UpdateModifedMetadata(name, metadatas);
        if (result.HasError) return new EmptyResult(result.Error!);

        var resultGitCommit = AppService.Instance.GitCommit($"Password metadata '{name}' updated");
        return resultGitCommit.HasError ? new EmptyResult(resultGitCommit.Error!) : new EmptyResult();
    }

    public EmptyResult EditStoreEntryPassword(string name, Password password)
    {
        if (!DoStoreEntryExists(name)) return new EmptyResult(new FsEntryNotFoundError());

        var result = UpdateModifedMetadata(name);
        if (result.HasError) return new EmptyResult(result.Error!);

        var filePath = GetEntryPath(name);
        var resultEncryptPassword = AppService.Instance.EncryptPassword(filePath, password);
        if (resultEncryptPassword.HasError) return new EmptyResult(resultEncryptPassword.Error!);

        var resultGitCommit = AppService.Instance.GitCommit($"Password '{name}' updated");
        return resultGitCommit.HasError ? new EmptyResult(resultGitCommit.Error!) : new EmptyResult();
    }

    public EmptyResult AddStoreEntry(string name, Password password)
    {
        if (DoStoreEntryExists(name))
            return new EmptyResult(new FsPasswordFileAlreadyExistsError());

        var filePath = GetEntryPath(name);
        if (name.Contains('/') || name.Contains('\\'))
        {
            var dirName = Path.GetDirectoryName(filePath)!;
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        var metadatasFilePath = GetMetadataPath(name);
        var resultEncryptMetadatas = AppService.Instance.EncryptMetadatas(metadatasFilePath, new MetadataCollection
        {
            new("created", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"), MetadataType.Internal),
            new("modified", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"), MetadataType.Internal),
        });
        if (resultEncryptMetadatas.HasError) return new EmptyResult(resultEncryptMetadatas.Error!);

        var resultEncryptPassword = AppService.Instance.EncryptPassword(filePath, password);
        if (!resultEncryptPassword.HasError)
        {
            var resultGitCommit = AppService.Instance.GitCommit($"Insert password '{name}')");
            return resultGitCommit.HasError ? new EmptyResult(resultGitCommit.Error!) : new EmptyResult();
        }

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

    public bool DoStoreEntryExists(string name, bool checkMetadatas = false)
    {
        return File.Exists(checkMetadatas ? GetMetadataPath(name) : GetEntryPath(name));
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
            Log.Error("Unable to acquire lock: {Message}", e.Message);
            return false;
        }

        return true;
    }

    public string GetStoreLocation()
    {
        if (!Directory.Exists(_storeFolderPath)) Directory.CreateDirectory(_storeFolderPath);
        return _storeFolderPath;
    }

    public Result<string, Error?> GetStoreId()
    {
        var path = Path.Join(GetStoreLocation(), GpgIdFileName);
        return !File.Exists(path)
            ? new Result<string, Error?>(new FsGpgIdKeyNotFoundError())
            : new Result<string, Error?>(File.ReadAllText(path));
    }

    public EmptyResult InitializeStoreFolder(string gpgId, string gitUrl)
    {
        if (IsStoreInitialized()) return new EmptyResult(new FsStoreAlreadyInitializedError());

        if (!AppService.Instance.GitClone(gitUrl))
        {
            return new EmptyResult(new GitCloneFailedError());
        }

        var createStoreResult = CreateStoreFolder();
        if (createStoreResult.HasError)
        {
            AppService.Instance.GitDeleteRepository();
            return new EmptyResult(createStoreResult.Error!);
        }

        var (isValid, error) = AppService.Instance.IsGpgIdValid(gpgId);

        if (error is not null)
        {
            AppService.Instance.GitDeleteRepository();
            return new EmptyResult(error);
        }

        if (!isValid)
        {
            AppService.Instance.GitDeleteRepository();
            return new EmptyResult(new GpgInvalidKeyError());
        }

        File.WriteAllText(Path.Join(_storeFolderPath, GpgIdFileName), gpgId);

        var gitCommitResult = AppService.Instance.GitCommit("Add '.gpg-id' file");
        if (gitCommitResult.HasError)
        {
            AppService.Instance.GitDeleteRepository();
            return new EmptyResult(gitCommitResult.Error!);
        }

        var resultGitIgnore = CreateGitIgnore();
        if (resultGitIgnore.HasError)
        {
            AppService.Instance.GitDeleteRepository();
            return new EmptyResult(resultGitIgnore.Error!);
        }

        return new EmptyResult();
    }

    public bool IsStoreInitialized()
    {
        if (!Directory.Exists(_storeFolderPath)) return false;

        var gpgIdFilePath = Path.Join(_storeFolderPath, GpgIdFileName);
        return File.Exists(gpgIdFilePath) && File.ReadAllText(gpgIdFilePath).Length != 0;
    }

    public void Initialize()
    {
    }

    #endregion

    #region Private methods

    private void EnumerateFilePaths(string current, List<Tuple<string, string>> filePaths)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var filePath in Directory.EnumerateFiles(current))
        {
            if (!filePath.EndsWith(".gpg")) continue;

            filePaths.Add(
                Tuple.Create(
                    filePath.Replace(GetStoreLocation(), string.Empty),
                    filePath
                )
            );
        }

        foreach (var dirPath in Directory.EnumerateDirectories(current))
        {
            if (dirPath.EndsWith(".git")) continue;

            EnumerateFilePaths(dirPath, filePaths);
        }
    }

    private EmptyResult CreateGitIgnore()
    {
        var path = Path.Join(GetStoreLocation(), ".gitignore");
        if (File.Exists(path))
        {
            var data = File.ReadAllLines(path).ToList();
            if (data.Any(t => t == AppLockFileName)) return new EmptyResult();

            data.Add(AppLockFileName);
            File.WriteAllText(path, string.Join("\n", data));
        }
        else
        {
            File.WriteAllText(path, AppLockFileName);
        }

        var gitCommitResult = AppService.Instance.GitCommit("Add lock file to'.gitignore' file");
        if (gitCommitResult.HasError)
        {
            AppService.Instance.GitDeleteRepository();
            return new EmptyResult(gitCommitResult.Error!);
        }

        return new EmptyResult();
    }

    private EmptyResult CreateStoreFolder()
    {
        if (Directory.Exists(_storeFolderPath))
        {
            var files = Directory.EnumerateFileSystemEntries(_storeFolderPath).ToList();
            if (files.Contains(GpgIdFileName)) return new EmptyResult(new FsStoreFolderAlreadyExistsError());
            if (files.Contains(AppLockFileName)) return new EmptyResult();

            return AppService.Instance.Encrypt(Path.Join(GetStoreLocation(), AppLockFileName), AppLockFileName);
        }

        Directory.CreateDirectory(_storeFolderPath);
        return AppService.Instance.Encrypt(Path.Join(GetStoreLocation(), AppLockFileName), AppLockFileName);
    }

    private string GetEntryPath(string name)
    {
        return Path.Join(GetStoreLocation(), $"{name}.gpg");
    }

    private EmptyResult UpdateModifedMetadata(string name, MetadataCollection? metadataCollection = null)
    {
        MetadataCollection metadatas;
        if (metadataCollection is null)
        {
            var (m, error) = RetrieveStoreEntryMetadatas(name);
            if (error is not null) return new EmptyResult(error);

            metadatas = m;
        }
        else
        {
            metadatas = metadataCollection;
        }

        var index = metadatas!.FindIndex(m => m.Key == "modified");

        if (index != -1)
        {
            metadatas[index].Value = DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss");
        }
        else
        {
            metadatas.Add(new Metadata("modified", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"),
                MetadataType.Internal));
        }

        var metadatasFilePath = GetMetadataPath(name);
        var resultEncryptMetadatas = AppService.Instance.EncryptMetadatas(metadatasFilePath, metadatas);
        return resultEncryptMetadatas.HasError ? new EmptyResult(resultEncryptMetadatas.Error!) : new EmptyResult();
    }

    private string GetMetadataPath(string name)
    {
        var path = GetEntryPath(name);
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

        return error is null && content.FromBase64() == AppLockFileName;
    }

    #endregion
}