using WinPass.Core;
using WinPass.Core.Services;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Tests;

public class ShowTests
{
    #region Constructors

    public ShowTests()
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
    public void ShowMetadatas_ShouldDisplayNothingDoesntExists()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        var (result, error) = AppService.Instance.GetMetadatas("random");

        // Assert
        Assert.NotNull(error);
        Assert.IsType<FsEntryNotFoundError>(error);
        Assert.Null(result);
        TestHelper.Done();
    }

    #endregion
}