using Horus.Core.Services;
using Horus.Shared.Models.Abstractions;

namespace Horus.Tests.Mocks;

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