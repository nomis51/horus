using WinPass.Core;
using WinPass.Core.Services;
using WinPass.Tests.Mocks;

namespace WinPass.Tests.Tests;

public class DestroyTests
{
    #region Constructors

    public DestroyTests()
    {
        AppService.Instance.Initialize(new AppServiceDependenciesProvider(
            new FsService(TestHelper.TestAppFolder),
            new GitService(),
            new GpgService(),
            new SettingsService()
        ));
    }

    #endregion

    #region Tests

    [Fact]
    public void DestroyStore_ShouldDestroyTheStore()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        File.WriteAllText(Path.Join(storePath, "test.gpg"), string.Empty);
        File.WriteAllText(Path.Join(storePath, "test.m.gpg"), string.Empty);
        
        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.DestroyStore();
        AppService.Instance.ReleaseLock();
        
        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        Assert.False(Directory.Exists(storePath));
    }

    #endregion
}