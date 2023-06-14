using System.Reflection.Metadata.Ecma335;
using Spectre.Console;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Errors.Gpg;
using WinPass.Shared.Models.Fs;

namespace WinPass.Core.Services;

public class FsService : IService
{
    #region Constants

    private const string StoreFolderName = ".password-store";
    private const string GpgIdFileName = ".gpg-id";
    private const string StoreFolderPathTemplate = $"%USERPROFILE%\\{StoreFolderName}\\";
    private readonly string _storeFolderPath;

    #endregion

    #region Constructors

    public FsService()
    {
        _storeFolderPath = Environment.ExpandEnvironmentVariables(StoreFolderPathTemplate);
    }

    #endregion

    #region Public methods

    public List<StoreEntry> SearchFiles(string term)
    {
        List<StoreEntry> entries = new();
        EnumerateGpgFiles(_storeFolderPath, entries, term);
        return entries;
    }

    public bool DoEntryExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public string GetGpgId()
    {
        var filePath = Path.Join(_storeFolderPath, GpgIdFileName);
        var data = File.ReadAllText(filePath).Trim();

        return data;
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

        var result = CreateStoreFolder();
        if (result.Item2 is not null) return result;

        if (!AppService.Instance.DoGpgKeyExists(gpgKey))
            return new ResultStruct<byte, Error?>(new GpgKeyNotFoundError());

        File.WriteAllText(Path.Join(_storeFolderPath, GpgIdFileName), gpgKey);
        return new ResultStruct<byte, Error?>(0);
    }

    public void Initialize()
    {
    }

    #endregion

    #region Private methods

    private bool IsStoreInitialized()
    {
        if (!Directory.Exists(_storeFolderPath)) return false;

        var gpgIdFilePath = Path.Join(_storeFolderPath, GpgIdFileName);
        return File.Exists(gpgIdFilePath) && File.ReadAllText(gpgIdFilePath).Length != 0;
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
                        if (!passwordMetadata.Key.Contains(searchText) && !passwordMetadata.Value.Contains(searchText)) continue;

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