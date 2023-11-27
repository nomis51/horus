using Horus.Shared.Models.Abstractions;

namespace Horus.Core.Services.Abstractions;

public interface IGitService : IService
{
    Result<string, Error?> GetRemoteRepositoryName();
    EmptyResult Push();
    ResultStruct<bool, Error?> IsAheadOfRemote();
    void DeleteRepository();
    bool Verify();
    bool Clone(string url);
    string Execute(string[] args);
    EmptyResult Commit(string message);
    EmptyResult Ignore(string filePath);
}