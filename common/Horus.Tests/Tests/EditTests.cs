using Horus.Core;
using Horus.Core.Services;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Errors.Fs;
using Horus.Tests.Mocks;

namespace Horus.Tests.Tests;

public class EditTests
{
    #region Constructors

    public EditTests()
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
    public void EditMetadatas_ShouldErrorDoesntExist()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.EditMetadatas("random", new MetadataCollection());
        AppService.Instance.ReleaseLock();

        // Assert
        TestHelper.Done();
        Assert.True(result.HasError);
        Assert.IsType<FsEntryNotFoundError>(result.Error);
    }

    [Fact]
    public void EditPassword_ShouldErrorDoesntExist()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.EditPassword("random", new Password("password"));
        AppService.Instance.ReleaseLock();

        // Assert
        TestHelper.Done();
        Assert.True(result.HasError);
        Assert.IsType<FsEntryNotFoundError>(result.Error);
    }

    [Fact]
    public void EditMetadatas_ShouldUpdateMetadatas()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var metadatas = new MetadataCollection
        {
            new("username", "tester")
        };
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), metadatas, TestHelper.TestGpgId);

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.EditMetadatas("test", new MetadataCollection
        {
            new("username", "better tester")
        });
        AppService.Instance.ReleaseLock();
        var (resultingMetadatas, error) = AppService.Instance.GetMetadatas("test");

        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        Assert.Null(error);
        Assert.True(resultingMetadatas.Any());
        Assert.Equal(metadatas[0].Key, resultingMetadatas[0].Key);
        Assert.NotEqual(metadatas[0].Value, resultingMetadatas[0].Value);
        Assert.Equal("better tester", resultingMetadatas[0].Value);
    }

    [Fact]
    public void EditPassword_ShouldUpdatePassword()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var password = new Password("password");
        AppService.Instance.EncryptPassword(Path.Join(storePath, "test.gpg"), password, TestHelper.TestGpgId);
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), new MetadataCollection
        {
            new("username", "tester")
        }, TestHelper.TestGpgId);

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var result = AppService.Instance.EditPassword("test", new Password("better password"));
        AppService.Instance.ReleaseLock();
        var (resultPassword, error) = AppService.Instance.GetPassword("test", false);

        // Assert
        TestHelper.Done();
        Assert.False(result.HasError);
        Assert.Null(error);
        Assert.NotEqual(password.ValueAsString, resultPassword!.ValueAsString);
        Assert.Equal("better password", resultPassword.ValueAsString);
    }
    
    [Fact]
    public void GeneratePassword_ShouldUpdatePassword()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        var storePath = TestHelper.GetStorePath();
        var password = new Password("password");
        AppService.Instance.EncryptPassword(Path.Join(storePath, "test.gpg"), password, TestHelper.TestGpgId);
        AppService.Instance.EncryptMetadatas(Path.Join(storePath, "test.m.gpg"), new MetadataCollection
        {
            new("username", "tester")
        }, TestHelper.TestGpgId);

        // Act
        Assert.True(AppService.Instance.AcquireLock());
        var (newPassword, error) = AppService.Instance.GenerateNewPassword(copy: false);
        AppService.Instance.EditPassword("test", newPassword!);
        AppService.Instance.ReleaseLock();
        var (resultPassword, error2) = AppService.Instance.DecryptPassword(Path.Join(storePath, "test.gpg"));

        // Assert
        TestHelper.Done();
        Assert.False(error is not null);
        Assert.Null(error2);
        Assert.NotEqual(password.ValueAsString, resultPassword!.ValueAsString);
    }

    #endregion
}