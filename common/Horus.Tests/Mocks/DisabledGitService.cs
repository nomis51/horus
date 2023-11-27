using Horus.Core.Services.Abstractions;
using Horus.Shared.Models.Abstractions;

namespace Horus.Tests.Mocks;

public class DisabledGitService : IGitService
{
    public void Initialize()
    {
    }

    public Result<string, Error?> GetRemoteRepositoryName()
    {
        return new Result<string, Error?>("RemoteRepository");
    }

    public EmptyResult Push()
    {
        return new EmptyResult();
    }

    public ResultStruct<bool, Error?> IsAheadOfRemote()
    {
        return new ResultStruct<bool, Error?>(false);
    }

    public void DeleteRepository()
    {
    }

    public bool Verify()
    {
        return true;
    }

    public bool Clone(string url)
    {
        return true;
    }

    public string Execute(string[] args)
    {
        return "some output";
    }

    public EmptyResult Commit(string message)
    {
        return new EmptyResult();
    }

    public EmptyResult Ignore(string filePath)
    {
        return new EmptyResult();
    }
}