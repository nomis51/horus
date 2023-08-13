using WinPass.Core.Services;
using WinPass.Shared.Models.Abstractions;

namespace WinPass.Tests.Mocks;

public class TestGpgIdFsService : FsService
{
    public TestGpgIdFsService(string folder) : base(folder)
    {
    }

    public override Result<string, Error?> GetStoreId()
    {
        return new Result<string, Error?>(TestHelper.TestGpgId);
    }
}