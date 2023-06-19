﻿using WinPass.Shared.Abstractions;
using WinPass.Shared.Helpers;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Errors.Gpg;
using WinPass.Shared.Models.Fs;
using System.Security.AccessControl;
using WinPass.Shared.Extensions;
using WinPass.Shared.Models;

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

    public ResultStruct<byte, Error?> SaveSettings(Settings settings)
    {
        var filePath = Path.Join(_storeFolderPath, ".settings");
        var value = settings.ToString();
        var id = GetGpgId();
        return AppService.Instance.Encrypt(id, filePath, value);
    }

    public Result<Settings?, Error?> GetSettings()
    {
        var filePath = Path.Join(_storeFolderPath, ".settings");
        return !File.Exists(filePath)
            ? new Result<Settings?, Error?>(new Settings())
            : AppService.Instance.DecryptSettings(filePath);
    }

    public string GetStorePath()
    {
        return _storeFolderPath;
    }

    public ResultStruct<byte, Error?> EditEntry(string name, Password password)
    {
        if (!DoEntryExists(name)) return new ResultStruct<byte, Error?>(new FsEntryNotFoundError());

        var filePath = GetPath(name);
        if (string.IsNullOrEmpty(password.Value))
        {
            var (existingPassword, errorExistingPassword) = AppService.Instance.GetPassword(name);
            if (errorExistingPassword is not null || existingPassword is null)
                return new ResultStruct<byte, Error?>(new FsEditPasswordFailedError());

            password.Set(existingPassword.Value);
            existingPassword.Dispose();
        }

        var filePathBak = $"{filePath}.bak";
        if (File.Exists(filePathBak)) File.Delete(filePathBak);

        File.Move(filePath, filePathBak);

        var (_, errorInsertPassword) = AppService.Instance.InsertPassword(name, password.ToString(), true);
        password.Dispose();
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

    public bool IsStoreInitialized()
    {
        if (!Directory.Exists(_storeFolderPath)) return false;

        var gpgIdFilePath = Path.Join(_storeFolderPath, GpgIdFileName);
        return File.Exists(gpgIdFilePath) && File.ReadAllText(gpgIdFilePath).Length != 0;
    }

    #endregion

    #region Private methods

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