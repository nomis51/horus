using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Data;

namespace Horus.Core.Services.Abstractions;

public interface IGpgService : IService
{
    bool Verify();
    EmptyResult Encrypt(string path, string value, string gpgId = "");
    Result<string, Error?> Decrypt(string path);
    EmptyResult EncryptMetadatas(string path, MetadataCollection metadatas, string gpgId = "");
    EmptyResult EncryptPassword(string path, Password password, string gpgId = "");
    Result<MetadataCollection?, Error?> DecryptMetadatas(string path);
    Result<List<MetadataCollection?>, Error?> DecryptManyMetadatas(List<Tuple<string, string>> items);
    Result<Password?, Error?> DecryptPassword(string path);
    ResultStruct<bool, Error?> IsIdValid(string id = "");
    Result<string, Error?> RestartGpgAgent();
    Result<string, Error?> StartGpgAgent();
    Result<string, Error?> StopGpgAgent();
}