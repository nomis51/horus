﻿using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Display;

namespace Horus.Core.Services.Abstractions;

public interface IFsService : IService
{
    EmptyResult ExportStore(string savePath);
    EmptyResult MigrateStore(string gpgId);
    Result<Password?, Error?> GenerateNewPassword(int length = 0, string customAlphabet = "", bool copy = false, bool dontReturn = false);
    Result<List<StoreEntry>, Error?> SearchStoreEntries(string text, bool searchMetadatas = false);
    Result<List<StoreEntry>, Error?> RetrieveStoreEntries();
    EmptyResult RemoveStoreEntry(string name);
    EmptyResult RenameStoreEntry(string name, string newName, bool duplicate = false);
    Result<Password?, Error?> RetrieveStoreEntryPassword(string name);
    Result<MetadataCollection?, Error?> RetrieveStoreEntryMetadatas(string name);
    EmptyResult EditStoreEntryMetadatas(string name, MetadataCollection metadatas);
    EmptyResult EditStoreEntryPassword(string name, Password password);
    EmptyResult AddStoreEntry(string name, Password password);
    bool DoStoreEntryExists(string name, bool checkMetadatas = false);
    EmptyResult DestroyStore();
    void ReleaseLock();
    bool AcquireLock();
    string GetStoreLocation();
    string GetAppLocation();
    string GetLogsLocation();
    Result<string, Error?> GetStoreId();
    EmptyResult InitializeStoreFolder(string gpgId, string gitUrl);
    bool IsStoreInitialized();
    EmptyResult SetPassphraseCacheTimeout(int timeout);
    bool VerifyLock();
    void Verify();
    EmptyResult CreateNewStore(string name);
    EmptyResult ChangeStore(string name);
    EmptyResult DeleteStore(string name);
    Result<List<string>, Error?> ListStores();
}