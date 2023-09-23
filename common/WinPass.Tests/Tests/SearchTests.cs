using WinPass.Core;
using WinPass.Core.Services;
using WinPass.Shared.Models.Data;
using WinPass.Tests.Mocks;

namespace WinPass.Tests.Tests;

public class SearchTests
{
    #region Constructors

    public SearchTests()
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
    public void SearchEntry_ShouldFindNothing()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var metadatas = new MetadataCollection
        {
            new("username", "flower123")
        };
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), metadatas, TestHelper.TestGpgId);

        // Act
        var (result, error) = AppService.Instance.SearchStoreEntries("wrong", true);

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.Empty(result);
    }

    [Fact]
    public void SearchEntry_ShouldFindEntryName()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var metadatas = new MetadataCollection
        {
            new("username", "flower123")
        };
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), metadatas, TestHelper.TestGpgId);

        // Act
        var (result, error) = AppService.Instance.SearchStoreEntries("test");

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.NotEmpty(result);
        Assert.Equal("test", result[0].Name);
        Assert.False(result[0].IsFolder);
        Assert.Empty(result[0].Entries);
        Assert.Empty(result[0].Metadata);
    }
    
    [Fact]
    public void SearchEntry_ShouldNotFindEntry()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var metadatas = new MetadataCollection
        {
            new("username", "flower123")
        };
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), metadatas, TestHelper.TestGpgId);

        // Act
        var (result, error) = AppService.Instance.SearchStoreEntries("wrong");

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.Empty(result);
    }

    [Fact]
    public void SearchEntry_ShouldFindMetadata()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var metadatas = new MetadataCollection
        {
            new("username", "flower123")
        };
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), metadatas, TestHelper.TestGpgId);

        // Act
        var (result, error) = AppService.Instance.SearchStoreEntries("flower", true);

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.NotEmpty(result);
        Assert.False(result[0].IsFolder);
        Assert.Empty(result[0].Entries);
        Assert.NotEmpty(result[0].Metadata);
        Assert.Equal("username: flower123", result[0].Metadata[0]);
    }
    
    [Fact]
    public void SearchEntry_ShouldNotFindMetadata()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var metadatas = new MetadataCollection
        {
            new("username", "flower123")
        };
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), metadatas, TestHelper.TestGpgId);

        // Act
        var (result, error) = AppService.Instance.SearchStoreEntries("flower");

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.Empty(result);
    }

    #endregion
}