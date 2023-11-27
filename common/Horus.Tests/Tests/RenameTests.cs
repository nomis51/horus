using Horus.Core;
using Horus.Core.Services;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Errors.Fs;
using Horus.Tests.Mocks;

namespace Horus.Tests.Tests;

public class RenameTests
{
    #region Constructors

    public RenameTests( )
    {
        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new TestGpgIdFsService(TestHelper.TestAppFolder),
            new DisabledGitService(),
            new GpgService(),
            new SettingsService()
        ));
    }

    #endregion

    #region Tests

    [Fact]
    public void RenameEntry_ShouldErrorEntryDoesntExist()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.RenameStoreEntry("random", "newRandom");
        AppService.Instance.ReleaseLock();

        // Assert
        TestHelper.Done();
        Assert.True(result.HasError);
        Assert.IsType<FsEntryNotFoundError>(result.Error);
    }

    [Fact]
    public void RenameEntry_ShouldRenameEntry()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var r = AppService.Instance.InsertPassword("test", new Password("password"));
        Assert.False(r.HasError);

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.RenameStoreEntry("test", "newTest");
        AppService.Instance.ReleaseLock();
        var (_, errorOld) = AppService.Instance.GetPassword("test", false);
        var (_, errorNew) = AppService.Instance.GetPassword("newTest", false);

        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        Assert.NotNull(errorOld);
        Assert.IsType<FsEntryNotFoundError>(errorOld);
        Assert.Null(errorNew);
    }
    
    [Fact]
    public void RenameEntry_ShouldDuplicateEntry()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var r = AppService.Instance.InsertPassword("test", new Password("password"));
        Assert.False(r.HasError);

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.RenameStoreEntry("test", "newTest", true);
        AppService.Instance.ReleaseLock();
        var (_, errorOld) = AppService.Instance.GetPassword("test", false);
        var (_, errorNew) = AppService.Instance.GetPassword("newTest", false);

        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        Assert.Null(errorOld);
        Assert.Null(errorNew);
    }

    #endregion
}