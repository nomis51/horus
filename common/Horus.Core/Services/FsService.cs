using System.IO.Compression;
using System.Runtime.InteropServices;
using Horus.Core.Services.Abstractions;
using Horus.Shared.Enums;
using Horus.Shared.Extensions;
using Horus.Shared.Helpers;
using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Display;
using Horus.Shared.Models.Errors.Fs;
using Horus.Shared.Models.Errors.Git;
using Horus.Shared.Models.Errors.Gpg;
using Serilog;

namespace Horus.Core.Services;

public class FsService : IFsService
{
    #region Constants

    private readonly string _appFolder;
    public const string GpgIdFileName = ".gpg-id";
    private const string AppLockFileName = ".lock";

    private const string StoreFolderName = "store";
    private const string MigrationStoreFolderName = "migration-store";
    private const string LogsFolderName = "logs";

    private readonly string _storeFolderPath;
    private readonly string _migrationStoreFolderPath;
    private readonly string _appFolderPath;
    private readonly string _logsFolderPath;

    #endregion

    #region Members

    private FileStream? _lockFileStream;

    #endregion

    #region Constructors

    public FsService(string appFolder = ".horus")
    {
        _appFolder = appFolder;

        _appFolderPath = Environment.ExpandEnvironmentVariables(
            $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{_appFolder}/");

        _appFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? _appFolderPath.Replace("/", "\\") : _appFolderPath.Replace("\\", "/");

        _storeFolderPath = Path.Join(_appFolderPath, StoreFolderName);
        _migrationStoreFolderPath = Path.Join(_appFolderPath, MigrationStoreFolderName);
        _logsFolderPath = Path.Join(_appFolderPath, LogsFolderName);
    }

    #endregion

    #region Public methods

    public EmptyResult ExportStore(string savePath)
    {
        if (!IsStoreInitialized()) return new EmptyResult(new FsStoreNotInitializedError());

        var filePath = Path.Join(savePath, $"horus-export-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip");
        ZipFile.CreateFromDirectory(GetStoreLocation(), filePath, CompressionLevel.NoCompression, false);

        return new EmptyResult();
    }

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
            var newMetadataFilePath = metadataFilePath.Replace(_appFolder, MigrationStoreFolderName);
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

            var newFilePath = filePath.Replace(_appFolder, MigrationStoreFolderName);
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
            File.Move(newFile, newFile.Replace(MigrationStoreFolderName, _appFolder));
        }

        File.WriteAllText(Path.Join(_storeFolderPath, GpgIdFileName), gpgId);

        if (Directory.Exists(_migrationStoreFolderPath)) Directory.Delete(_migrationStoreFolderPath, true);

        var resultGitCommit = AppService.Instance.GitCommit($"Migrate store to new GPG keypair '{gpgId}'");
        return resultGitCommit.HasError ? new EmptyResult(resultGitCommit.Error!) : new EmptyResult();
    }

    public Result<Password?, Error?> GenerateNewPassword(int length = 0, string customAlphabet = "", bool copy = false,
        bool dontReturn = false)
    {
        var (settings, error) = AppService.Instance.GetSettings();
        if (error is not null) return new Result<Password?, Error?>(error);

        var newPassword = new Password(
            PasswordHelper.Generate(
                length <= 0 ? settings!.DefaultLength : length,
                string.IsNullOrWhiteSpace(customAlphabet) ? settings!.DefaultCustomAlphabet : customAlphabet
            )
        );

        if (copy)
        {
            ClipboardHelper.Copy(newPassword.ValueAsString);
        }

        if (!dontReturn) return new Result<Password?, Error?>(newPassword);

        newPassword.Dispose();
        return new Result<Password?, Error?>(default(Password));
    }

    public Result<List<StoreEntry>, Error?> SearchStoreEntries(string text, bool searchMetadatas = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new Result<List<StoreEntry>, Error?>(Enumerable.Empty<StoreEntry>().ToList());

        List<Tuple<string, string>> items = new();
        var storePath = GetStoreLocation();

        EnumerateFilePaths(storePath, items);
        if (items.Count == 0) return new Result<List<StoreEntry>, Error?>(Enumerable.Empty<StoreEntry>().ToList());

        List<MetadataCollection> metadatas = new();

        if (searchMetadatas)
        {
            var (lstMetadatas, error) =
                AppService.Instance.DecryptManyMetadatas(items.Where(m => m.Item2.EndsWith(".m.gpg")).ToList());
            if (error is not null)
                return new Result<List<StoreEntry>, Error?>(error);

            metadatas = lstMetadatas!;
        }

        var loweredText = text.Trim().ToLower();
        List<StoreEntry> entries = new();
        LocalSearchEntries(storePath, string.Empty);

        return new Result<List<StoreEntry>, Error?>(entries);

        void LocalSearchEntries(string current, string currenPath)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var filePath in Directory.EnumerateFiles(current))
            {
                if (!filePath.EndsWith(".m.gpg")) continue;

                var path = Path.GetFileName(filePath);

                List<string> metadataFound = new();
                if (searchMetadatas)
                {
                    var currentEntryPath = string.IsNullOrEmpty(currenPath) ? path : $"{currenPath}/{path}";
                    var entryMetadatas = metadatas.FirstOrDefault(m => m?.Name == currentEntryPath);

                    if (entryMetadatas is not null)
                    {
                        foreach (var metadata in entryMetadatas)
                        {
                            if (!metadata.Key.ToLower().Contains(loweredText) &&
                                !metadata.Value.ToLower().Contains(loweredText)) continue;

                            metadataFound.Add(metadata.ToString());
                        }
                    }
                }

                var name = path.Split(".").First();
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

        LocalEnumerateEntries(storePath, ref results);

        return new Result<List<StoreEntry>, Error?>(results);

        void LocalEnumerateEntries(string current, ref List<StoreEntry> entries)
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

                List<StoreEntry> subEntries = new();
                LocalEnumerateEntries(dirPath, ref subEntries);
                entries[^1].Entries.AddRange(subEntries);

                if (entries.Last().Entries.Count == 0)
                {
                    entries.Remove(entries.Last());
                }
            }

            entries = entries.OrderBy(e => e.Name).ToList();
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
        if (!DoStoreEntryExists(name, true)) return new Result<MetadataCollection?, Error?>(new FsEntryNotFoundError());

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
            var resultGitCommit = AppService.Instance.GitCommit($"Insert password '{name}'");
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

    public string GetAppLocation()
    {
        return _appFolderPath;
    }

    public string GetLogsLocation()
    {
        return _logsFolderPath;
    }

    public virtual Result<string, Error?> GetStoreId()
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

        var createStoreResult = CreateStoreFolder(gpgId);
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

    public EmptyResult SetPassphraseCacheTimeout(int timeout)
    {
        var confFilePath = Environment.ExpandEnvironmentVariables(Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "AppData/Roaming/gnupg/gpg-agent.conf"
                : ".gnupg/gpg-agent.conf"));

        if (!File.Exists(confFilePath))
        {
            File.WriteAllText(confFilePath, $"max-cache-ttl {timeout}\ndefault-cache-ttl {timeout}");
        }
        else
        {
            var lines = File.ReadAllLines(confFilePath).ToList();
            var index = lines.FindIndex(l => l.StartsWith("max-cache-ttl"));

            if (index == -1)
            {
                lines.Add($"max-cache-ttl {timeout}");
            }
            else
            {
                lines[index] = $"max-cache-ttl {timeout}";
                index = lines.FindIndex(l => l.StartsWith("default-cache-ttl"));
                if (index == -1)
                {
                    lines.Add($"default-cache-ttl {timeout}");
                }
                else
                {
                    lines[index] = $"default-cache-ttl {timeout}";
                }
            }

            File.WriteAllLines(confFilePath, lines);
        }

        var (_, error) = AppService.Instance.RestartGpgAgent();
        return error is not null ? new EmptyResult(error) : new EmptyResult();
    }

    public bool VerifyLock()
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

    public void Verify()
    {
        var confFilePath = Environment.ExpandEnvironmentVariables(Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "AppData/Roaming/gnupg/gpg-agent.conf"
                : ".gnupg/gpg-agent.conf"));

        if (!File.Exists(confFilePath))
        {
            File.WriteAllText(confFilePath, "no-allow-external-cache");
        }
        else
        {
            var lines = File.ReadAllLines(confFilePath).ToList();
            var index = lines.FindIndex(l => l.StartsWith("no-allow-external-cache"));

            if (index != -1) return;

            lines.Add("no-allow-external-cache");
            File.WriteAllLines(confFilePath, lines);
            AppService.Instance.RestartGpgAgent();
        }
    }

    public EmptyResult CreateNewStore(string name)
    {
        var result = AppService.Instance.GitCreateBranch(name);
        if (result.HasError) return result;

        var result2 = AppService.Instance.GitChangeBranch(name);
        return result2;
    }

    public EmptyResult ChangeStore(string name)
    {
        return AppService.Instance.GitChangeBranch(name);
    }

    public EmptyResult DeleteStore(string name)
    {
        return AppService.Instance.GitRemoveBranch(name);
    }

    public Result<List<string>, Error?> ListStores()
    {
        var (stores, error) = AppService.Instance.GitListBranches();
        if (error is not null) return new Result<List<string>, Error?>(error);

        return new Result<List<string>, Error?>(
            stores.Select(s => s.StartsWith("master") ? "main" : s)
                .ToList()
        );
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
                    filePath.Replace(GetStoreLocation(), string.Empty)
                        .Trim('\\')
                        .Trim('/'),
                    filePath
                )
            );
        }

        foreach (var dirPath in Directory.EnumerateDirectories(current))
        {
            if (dirPath.EndsWith(".git")) continue;

            EnumerateFilePaths(dirPath, filePaths);
        }

        filePaths = filePaths.OrderBy(e => e.Item2)
            .ToList();
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

    private EmptyResult CreateStoreFolder(string gpgId)
    {
        if (Directory.Exists(_storeFolderPath))
        {
            var files = Directory.EnumerateFileSystemEntries(_storeFolderPath).ToList();
            if (files.Contains(GpgIdFileName)) return new EmptyResult(new FsStoreFolderAlreadyExistsError());
            if (files.Contains(AppLockFileName)) return new EmptyResult();

            return AppService.Instance.Encrypt(Path.Join(GetStoreLocation(), AppLockFileName), AppLockFileName, gpgId);
        }

        Directory.CreateDirectory(_storeFolderPath);
        return AppService.Instance.Encrypt(Path.Join(GetStoreLocation(), AppLockFileName), AppLockFileName, gpgId);
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

            metadatas = m!;
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

    #endregion
}