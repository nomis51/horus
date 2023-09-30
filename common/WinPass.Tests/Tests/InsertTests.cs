using WinPass.Core;
using WinPass.Core.Services;
using WinPass.Shared.Models.Data;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Tests.Mocks;

namespace WinPass.Tests.Tests;

public class InsertTests
{
    #region Constructors

    public InsertTests()
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
    public void InsertEntry_ShouldErrorEntryAlreadyExists()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        File.WriteAllText(Path.Join(TestHelper.GetStorePath(), "test.gpg"), "random");

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.InsertPassword("test", new Password("random"));
        AppService.Instance.ReleaseLock();

        // Assert
        TestHelper.Done();
        Assert.True(result.HasError);
        Assert.IsType<FsPasswordFileAlreadyExistsError>(result.Error);
    }
    
    [Fact]
    public void InsertEntry_ShouldInsertPassword()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.InsertPassword("test", new Password("random"));
        AppService.Instance.ReleaseLock();
        var (resultPassword, error) = AppService.Instance.GetPassword("test", false);

        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        Assert.Null(error);
        Assert.Equal("random", resultPassword!.ValueAsString);
    }

    #endregion
}