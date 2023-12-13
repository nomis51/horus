using Horus.Shared.Models.Abstractions;

namespace Horus.Core.Services.Abstractions;

public interface IGitService : IService
{
    Result<string, Error?> GetRemoteRepositoryName();
    EmptyResult Push();
    EmptyResult Pull();
    Result<Tuple<int,int>, Error?> Fetch();
    ResultStruct<bool, Error?> IsAheadOfRemote();
    void DeleteRepository();
    bool Verify();
    bool Clone(string url);
    string Execute(string[] args);
    EmptyResult Commit(string message);
    EmptyResult Ignore(string filePath);
    EmptyResult CreateBranch(string name);
    EmptyResult ChangeBranch(string name);
    EmptyResult RemoveBranch(string name);
    Result<List<string>, Error?> ListBranches();
    Result<string, Error?> GetCurrentBranch();
}