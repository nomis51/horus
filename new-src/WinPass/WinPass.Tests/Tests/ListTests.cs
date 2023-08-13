using WinPass.Core;
using WinPass.Core.Services;
using WinPass.Shared.Models.Display;
using WinPass.Tests.Mocks;

namespace WinPass.Tests.Tests;

public class ListTests
{
    #region Constructors

    public ListTests()
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
    public void ListStoreEntries_ShouldDisplayNothing()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        
        // Act
        var (entries, error) = AppService.Instance.GetStoreEntries();

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.Empty(entries);
    }

    [Fact]
    public void ListStoreEntries_ShouldDisplayEntries()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        File.WriteAllText(Path.Join(storePath, "a.gpg"), string.Empty);
        File.WriteAllText(Path.Join(storePath, "b.gpg"), string.Empty);
        File.WriteAllText(Path.Join(storePath, "c.gpg"), string.Empty);

        Directory.CreateDirectory(Path.Join(storePath, "folderA"));
        Directory.CreateDirectory(Path.Join(storePath, "folderB"));
        Directory.CreateDirectory(Path.Join(storePath, "folderC"));

        Directory.CreateDirectory(Path.Join(storePath, "folderC", "subFolderA"));

        File.WriteAllText(Path.Join(storePath, "folderA", "d.gpg"), string.Empty);
        File.WriteAllText(Path.Join(storePath, "folderA", "e.gpg"), string.Empty);
        File.WriteAllText(Path.Join(storePath, "folderC", "f.gpg"), string.Empty);
        File.WriteAllText(Path.Join(storePath, "folderC", "subFolderA", "g.gpg"), string.Empty);

        List<StoreEntry> expectedEntries = new()
        {
            new StoreEntry("a"),
            new StoreEntry("b"),
            new StoreEntry("c"),
            new StoreEntry("folderA", true)
            {
                Entries =
                {
                    new StoreEntry("d"),
                    new StoreEntry("e")
                }
            },
            new StoreEntry("folderC", true)
            {
                Entries =
                {
                    new StoreEntry("f"),
                    new StoreEntry("subFolderA", true)
                    {
                        Entries =
                        {
                            new StoreEntry("g"),
                        }
                    }
                }
            }
        };

        // Act
        var (entries, error) = AppService.Instance.GetStoreEntries();

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.NotEmpty(entries);
        Assert.Equivalent(expectedEntries, entries);
    }

    #endregion
}