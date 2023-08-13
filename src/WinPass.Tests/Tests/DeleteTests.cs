using WinPass.Core;
using WinPass.Core.Services;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Tests.Mocks;

namespace WinPass.Tests.Tests;

public class DeleteTests
{
    #region Constructors

    public DeleteTests()
    {
        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new FsService(TestHelper.TestAppFolder),
            new DisabledGitService(),
            new GpgService(),
            new SettingsService()
        ));
    }

    #endregion

    #region Tests

    [Fact]
    public void DeleteEntry_ShouldDeleteTheEntry()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        File.WriteAllText(Path.Join(storePath, "test.gpg"), string.Empty);
        File.WriteAllText(Path.Join(storePath, "test.m.gpg"), string.Empty);

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.DeleteStoreEntry("test");
        AppService.Instance.ReleaseLock();

        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        Assert.False(File.Exists(Path.Join(storePath, "test.gpg")));
    }

    [Fact]
    public void DeleteEntry_ShouldErrorDoesntExists()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.DeleteStoreEntry("random");
        AppService.Instance.ReleaseLock();

        // Assert
        TestHelper.Done();
        Assert.True(result.HasError);
        Assert.IsType<FsEntryNotFoundError>(result.Error);
    }

    #endregion
}