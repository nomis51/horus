using Horus.Core;
using Horus.Core.Services;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Errors.Fs;
using Horus.Tests.Mocks;
using TextCopy;

namespace Horus.Tests.Tests;

public class ShowTests
{
    #region Constructors

    public ShowTests()
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
    public void ShowMetadatas_ShouldDisplayNothingDoesntExists()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        var (result, error) = AppService.Instance.GetMetadatas("random");

        // Assert
        TestHelper.Done();
        Assert.NotNull(error);
        Assert.IsType<FsEntryNotFoundError>(error);
        Assert.Null(result);
    }

    [Fact]
    public void ShowPassword_ShouldDisplayNothingDoesntExists()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();

        // Act
        var (result, error) = AppService.Instance.GetPassword("random");

        // Assert
        TestHelper.Done();
        Assert.NotNull(error);
        Assert.IsType<FsEntryNotFoundError>(error);
        Assert.Null(result);
    }

    [Fact]
    public void ShowMetadatas_ShouldDisplayMetadatas()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        MetadataCollection metadatas = new()
        {
            new Metadata("username", "user123")
        };
        var e = AppService.Instance.EncryptMetadatas(Path.Join(TestHelper.GetStorePath(), "test.m.gpg"), metadatas,
            TestHelper.TestGpgId);
        Assert.False(e.HasError);

        // Act
        var (result, error) = AppService.Instance.GetMetadatas("test");

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.Equivalent(metadatas, result);
    }

    [Fact]
    public void ShowPassword_ShouldDisplayThePassword()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        Password password = new("supersecret");
        var e = AppService.Instance.EncryptPassword(Path.Join(TestHelper.GetStorePath(), "test.gpg"), password,
            TestHelper.TestGpgId);
        Assert.False(e.HasError);

        // Act
        var (result, error) = AppService.Instance.GetPassword("test", false);

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.Equivalent(password, result);
    }

    [Fact]
    public void ShowPassword_ShouldDisplayNothingButCopyToClipboard()
    {
        // Arrange
        TestHelper.EnsureReady();
        TestHelper.CreateTestFolder();
        Password password = new("supersecret");
        var e = AppService.Instance.EncryptPassword(Path.Join(TestHelper.GetStorePath(), "test.gpg"), password,
            TestHelper.TestGpgId);
        Assert.False(e.HasError);

        // Act
        var (result, error) = AppService.Instance.GetPassword("test", true, 2);

        // Assert
        TestHelper.Done();
        Assert.Null(error);
        Assert.Null(result!.Value);
        Assert.Equivalent(password.ValueAsString, ClipboardService.GetText());
        Thread.Sleep(4000);
        Assert.Equal(string.Empty, ClipboardService.GetText());
    }

    #endregion
}