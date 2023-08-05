using WinPass.Shared.Abstractions;
using WinPass.Shared.Enums;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Errors.Gpg;
using WinPass.Shared.Models.Fs;
using WinPass.Shared.Models;

namespace WinPass.Core.Services;

public class FsService : IService
{
    #region Constants

    private const string WinpassEnvVariableName = "WINPASS_SETTINGS";
    private const string StoreFolderName = ".password-store";
    private const string GpgIdFileName = ".gpg-id";
    private const string AppLockFileName = ".lock";
    public const string GpgLockContent = "lock";

    private readonly string _storeFolderPathTemplate =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{StoreFolderName}/";

    private readonly string _storeFolderPath;

    #endregion

    #region Members

    private FileStream? _lockFileStream;
    private Settings? _settings;

    #endregion

    #region Constructors

    public FsService()
    {
        _storeFolderPath = Environment.ExpandEnvironmentVariables(_storeFolderPathTemplate);
    }

    #endregion

    #region Public methods

    public bool AcquireLock()
    {
        if (_lockFileStream is not null) return false;

        var filePath = Path.Join(GetStorePath(), AppLockFileName);
        if (!File.Exists(filePath))
        {
            var id = GetGpgId();
            var gpg = new Gpg.Gpg(id);
            if (string.IsNullOrEmpty(id)) return false;

            var (_, error) = AppService.Instance.Encrypt(gpg, filePath, GpgLockContent);
            if (error is not null) return false;

            AppService.Instance.GitIgnore(filePath);
        }

        try
        {
            _lockFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public void ReleaseLock()
    {
        if (_lockFileStream is null) return;
        _lockFileStream.Close();
        _lockFileStream = null;
    }

    public ResultStruct<byte, Error?> DestroyStore()
    {
        var id = GetGpgId();
        if (string.IsNullOrEmpty(id)) return new ResultStruct<byte, Error?>(new FsGpgIdKeyNotFoundError());

        var gpg = new Gpg.Gpg(id);
        if (!VerifyLock(gpg)) return new ResultStruct<byte, Error?>(new GpgDecryptLockFileError());

        AppService.Instance.ReleaseLock();
        AppService.Instance.DeleteRepository(GetStorePath());
        return new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<byte, Error?> SaveSettings(Settings settings)
    {
        var data = settings.ToString();
        Environment.SetEnvironmentVariable(WinpassEnvVariableName, data, EnvironmentVariableTarget.User);
        return new ResultStruct<byte, Error?>(0);

        // var filePath = Path.Join(_storeFolderPath, ".settings");
        // var value = settings.ToString();
        //
        // File.WriteAllText(filePath, value);
        // return new ResultStruct<byte, Error?>(0);
    }

    public Result<Settings?, Error?> GetSettings()
    {
        if (_settings is not null) return new Result<Settings?, Error?>(_settings);

        var data = Environment.GetEnvironmentVariable(WinpassEnvVariableName, EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(data)) return new Result<Settings?, Error?>(new Settings());

        var settings = new Settings();
        var parts = data.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var values = part.Split("=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 2) continue;

            switch (values[0])
            {
                case nameof(Settings.ClearTimeout):
                    if (!int.TryParse(values[1], out var clearTimeout)) continue;
                    settings.ClearTimeout = clearTimeout;
                    break;

                case nameof(Settings.DefaultLength):
                    if (!int.TryParse(values[1], out var defaultLength)) continue;
                    settings.DefaultLength = defaultLength;
                    break;

                case nameof(Settings.DefaultCustomAlphabet):
                    settings.DefaultCustomAlphabet = values[1];
                    break;

                case nameof(Settings.Language):
                    settings.Language = values[1];
                    break;
            }
        }

        _settings = settings;

        return new Result<Settings?, Error?>(_settings);

        // var filePath = Path.Join(_storeFolderPath, ".settings");
        // if (!File.Exists(filePath)) return new Result<Settings?, Error?>(new Settings());
        //
        // var data = File.ReadAllText(filePath);
        //
        // try
        // {
        //     var settings = JsonConvert.DeserializeObject<Settings>(data);
        //     return new Result<Settings?, Error?>(settings);
        // }
        // catch (Exception e)
        // {
        //     Log.Error("Unable to parse settings: {Message}", e.Message);
        // }
        //
        // return new Result<Settings?, Error?>(new Settings());
    }

    public string GetStorePath()
    {
        return _storeFolderPath;
    }

    public ResultStruct<byte, Error?> InsertEntry(string name, Password password)
    {
        var gpgKeyId = GetGpgId();
        if (string.IsNullOrEmpty(gpgKeyId)) return new ResultStruct<byte, Error?>(new FsGpgIdKeyNotFoundError());

        var gpg = new Gpg.Gpg(gpgKeyId);

        if (DoEntryExists(name))
            return new ResultStruct<byte, Error?>(new FsPasswordFileAlreadyExistsError());

        var filePath = GetPath(name);
        if (name.Contains('/') || name.Contains('\\'))
        {
            var dirName = Path.GetDirectoryName(filePath)!;
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        var createdMetadata =
            password.Metadata.FirstOrDefault(m => m is { Type: MetadataType.Internal, Key: "created" });
        if (createdMetadata is null)
        {
            password.Metadata.Add(new Metadata("created", $"{DateTime.Now:yyyy-MM-dd HH':'mm':'ss}",
                MetadataType.Internal));
        }

        var (_, errorEncrypt) = AppService.Instance.Encrypt(gpg, filePath, password.ToString());
        if (!DoEntryExists(name))
            return new ResultStruct<byte, Error?>(new GpgEncryptError("Resulting entry not found"));

        return errorEncrypt is not null
            ? new ResultStruct<byte, Error?>(errorEncrypt)
            : new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<byte, Error?> EditEntry(string name, Password password)
    {
        if (!DoEntryExists(name)) return new ResultStruct<byte, Error?>(new FsEntryNotFoundError());

        var filePath = GetPath(name);
        if (password.ValueBytes is null || password.ValueBytes.Length == 0)
        {
            var (existingPassword, errorExistingPassword) = AppService.Instance.GetPassword(name);
            if (errorExistingPassword is not null || existingPassword is null)
                return new ResultStruct<byte, Error?>(new FsEditPasswordFailedError());

            password.ValueBytes = existingPassword.ValueBytes;
            existingPassword.Dispose();
            existingPassword = null;
            GC.Collect();
        }

        var filePathBak = $"{filePath}.bak";
        if (File.Exists(filePathBak)) File.Delete(filePathBak);

        File.Move(filePath, filePathBak);

        var modifiedMetadata =
            password.Metadata.FirstOrDefault(m => m is { Type: MetadataType.Internal, Key: "modified" });
        if (modifiedMetadata is not null)
        {
            modifiedMetadata.Value = $"{DateTime.Now:yyyy-MM-dd HH':'mm':'ss}";
        }
        else
        {
            password.Metadata.Add(new Metadata("modified", $"{DateTime.Now:yyyy-MM-dd HH':'mm':'ss}",
                MetadataType.Internal));
        }

        var (_, errorInsertPassword) = AppService.Instance.InsertPassword(name, password, true);
        password.Dispose();
        password = null;
        GC.Collect();
        if (errorInsertPassword is not null)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            File.Move(filePathBak, filePath);
            return new ResultStruct<byte, Error?>(errorInsertPassword);
        }

        File.Delete(filePathBak);
        return new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<byte, Error?> RenameEntry(string name, string newName, bool duplicate = false)
    {
        if (!DoEntryExists(name)) return new ResultStruct<byte, Error?>(new FsEntryNotFoundError());

        var filePath = GetPath(name);
        var newFilePath = GetPath(newName);

        if (duplicate)
        {
            File.Copy(filePath, newFilePath);
        }
        else
        {
            File.Move(filePath, newFilePath);
        }

        return new ResultStruct<byte, Error?>(0);
    }

    public ResultStruct<byte, Error?> DeleteEntry(string name)
    {
        if (!DoEntryExists(name)) return new ResultStruct<byte, Error?>(new FsEntryNotFoundError());

        var filePath = GetPath(name);
        File.Delete(filePath);
        return new ResultStruct<byte, Error?>(0);
    }

    public List<StoreEntry> SearchFiles(string term)
    {
        List<StoreEntry> entries = new();
        EnumerateGpgFiles(_storeFolderPath, entries, term);
        return entries;
    }

    public bool DoEntryExists(string name)
    {
        var filePath = GetPath(name);
        return File.Exists(filePath);
    }

    public string GetGpgId()
    {
        var filePath = Path.Join(_storeFolderPath, GpgIdFileName);
        return !File.Exists(filePath) ? string.Empty : File.ReadAllText(filePath).Trim();
    }

    public string GetPath(string name)
    {
        return Path.Join(_storeFolderPath, $"{name}.gpg");
    }

    public Result<List<StoreEntry>?, Error?> ListStoreEntries()
    {
        if (!IsStoreInitialized())
            return new Result<List<StoreEntry>?, Error?>(new FsStoreNotInitializedError());

        List<StoreEntry> entries = new();
        EnumerateGpgFiles(_storeFolderPath, entries);
        return new Result<List<StoreEntry>?, Error?>(entries);
    }

    public ResultStruct<byte, Error?> InitializeStoreFolder(string gpgKey)
    {
        if (IsStoreInitialized()) return new ResultStruct<byte, Error?>(new FsStoreAlreadyInitializedError());
        var gpg = new Gpg.Gpg(gpgKey);

        var result = CreateStoreFolder();
        if (result.Item2 is not null) return result;

        var (ok, error) = AppService.Instance.IsKeyValid(gpg);

        if (error is not null) return new ResultStruct<byte, Error?>(error);
        if (!ok) return new ResultStruct<byte, Error?>(new GpgInvalidKeyError());

        File.WriteAllText(Path.Join(_storeFolderPath, GpgIdFileName), gpgKey);
        return new ResultStruct<byte, Error?>(0);
    }

    public void Initialize()
    {
    }

    public bool IsStoreInitialized()
    {
        if (!Directory.Exists(_storeFolderPath)) return false;

        var gpgIdFilePath = Path.Join(_storeFolderPath, GpgIdFileName);
        return File.Exists(gpgIdFilePath) && File.ReadAllText(gpgIdFilePath).Length != 0;
    }

    #endregion

    #region Private methods

    private bool VerifyLock(Gpg.Gpg gpg)
    {
        if (_lockFileStream is null) return false;

        var filePath = Path.Join(_storeFolderPath, AppLockFileName);
        if (!File.Exists(filePath)) return false;

        _lockFileStream.Position = 0;
        using var ms = new MemoryStream();
        _lockFileStream.CopyTo(ms);
        var tmpFile = Path.GetTempFileName();
        File.WriteAllBytes(tmpFile, ms.ToArray());

        var (_, error) = AppService.Instance.DecryptLock(gpg, tmpFile);
        File.Delete(tmpFile);

        return error is null;
    }

    private void EnumerateGpgFiles(string path, List<StoreEntry> entries, string searchText = "")
    {
        foreach (var filePath in Directory.EnumerateFiles(path))
        {
            if (!filePath.EndsWith(".gpg")) continue;

            var doSearch = !string.IsNullOrEmpty(searchText);
            var fileName = Path.GetFileName(filePath).Split(".gpg").First();

            List<string> metadata = new();
            if (doSearch)
            {
                var name = Path.Join(path, fileName)[_storeFolderPath.Length..];
                var (password, error) = AppService.Instance.GetPassword(name);
                if (error is null && password is not null)
                {
                    foreach (var passwordMetadata in password.Metadata)
                    {
                        if (!passwordMetadata.Key.Contains(searchText) && !passwordMetadata.Value.Contains(searchText))
                            continue;

                        metadata.Add(passwordMetadata.ToString());
                    }
                }
            }

            if (doSearch && !fileName.Contains(searchText) && !metadata.Any()) continue;

            entries.Add(
                new StoreEntry(
                    fileName,
                    highlight: doSearch && (fileName.Contains(searchText) || metadata.Any()),
                    metadata: metadata
                )
            );
        }

        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            if (dir.StartsWith(".git")) continue;
            var dirName = Path.GetFileName(dir);

            entries.Add(
                new StoreEntry(dirName, true, !string.IsNullOrEmpty(searchText) && dirName.Contains(searchText)));

            var lastEntry = entries.Last();
            EnumerateGpgFiles(dir, lastEntry.Entries, searchText);

            if (!lastEntry.Entries.Any() && !lastEntry.Highlight)
            {
                entries.RemoveAt(entries.Count - 1);
            }
        }
    }

    private ResultStruct<byte, Error?> CreateStoreFolder()
    {
        if (Directory.Exists(_storeFolderPath))
        {
            var files = Directory.EnumerateFileSystemEntries(_storeFolderPath).ToList();
            return files.Contains(GpgIdFileName)
                ? new ResultStruct<byte, Error?>(new FsStoreFolderAlreadyExistsError())
                : new ResultStruct<byte, Error?>(0);
        }

        Directory.CreateDirectory(_storeFolderPath);
        return new ResultStruct<byte, Error?>(0);
    }

    #endregion
}