using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Display;

namespace WinPass.Core.Services.Abstractions;

public interface IFsService : IService
{
    EmptyResult MigrateStore(string gpgId);
    EmptyResult GenerateNewPassword(string name, int length = 0, string customAlphabet = "");
    Result<List<StoreEntry>, Error?> SearchStoreEntries(string text);
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
    Result<string, Error?> GetStoreId();
    EmptyResult InitializeStoreFolder(string gpgId, string gitUrl);
    bool IsStoreInitialized();
}