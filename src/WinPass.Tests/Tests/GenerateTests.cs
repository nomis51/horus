using WinPass.Core;
using WinPass.Core.Services;
using WinPass.Tests.Mocks;

namespace WinPass.Tests.Tests;

public class GenerateTests
{
    #region Constructors

    public GenerateTests()
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
    public void GeneratePassword_ShouldCreateNewGeneratedEntry()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.GenerateNewPassword(copy: false);
        AppService.Instance.ReleaseLock();
        var (resultPassword, error) = AppService.Instance.DecryptPassword(Path.Join(storePath, "test.gpg"));

        // Assert
        TestHelper.Done();
        Assert.False(result.Item2 is not null);
        Assert.Null(error);
        Assert.NotNull(resultPassword);
    }

    #endregion
}