using Horus.Core;
using Horus.Core.Services;
using Horus.Tests.Mocks;

namespace Horus.Tests.Tests;

public class ExportTests
{
    #region Constructors

    public ExportTests()
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
    public void ExportStore_ShouldExportTheStoreToPath()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        File.WriteAllText(Path.Join(storePath, "a.gpg"), string.Empty);

        // Act
        var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop",
            "winpass-unittests");
        if (Directory.Exists(path)) Directory.Delete(path, true);
        Directory.CreateDirectory(path);
        var result = AppService.Instance.ExportStore(path);

        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        var files = Directory.EnumerateFiles(path);
        Assert.NotNull(files.FirstOrDefault(e => e.Contains("winpass-export-") && e.EndsWith(".zip")));
        Directory.Delete(path, true);
    }

    #endregion
}