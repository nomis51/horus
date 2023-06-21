using System.Diagnostics;
using System.Text;
using WinPass.Core.Services;
using WinPass.Shared.Models;

namespace WinPass.Tests.Tests;

public class InsertTests : Test
{
    #region Members

    private readonly Cli _sut;
    private const string GpgId = "F76E20A0A9DF6C1F236D1DC5F8DFEDDF0640812A";
    private readonly string _storePath;
    private readonly string _gpgIdFilePath;

    #endregion

    #region Constructors

    public InsertTests(StringBuilder stdout) : base(stdout)
    {
        _sut = new Cli();
        _storePath = Environment.ExpandEnvironmentVariables("%USERPROFILE%/.password-store");
        _gpgIdFilePath = Path.Join(_storePath, ".gpg-id");
    }

    #endregion

    #region Tests

    public void ShouldErrorWithPasswordNameRequired()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        _sut.Run(new[] { "init", GpgId });

        // Act
        _sut.Run(new[] { "insert" });

        // Assert
        Debug.Assert(Stdout.ToString().Contains("Password name argument required"));
    }

    public void ShouldErrorWithPasswordNameAlreadyExists()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        _sut.Run(new[] { "init", GpgId });
        File.WriteAllText(Path.Join(_storePath, "test.gpg"), string.Empty);

        // Act
        _sut.Run(new[] { "insert", "test" });

        // Assert
        Debug.Assert(Stdout.ToString().Contains("Password for test already exists"));
    }

    public void ShouldInsertThePassword()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        _sut.Run(new[] { "init", GpgId });

        // Act
        var (_, error) = AppService.Instance.InsertPassword("test", new Password("test"));

        // Assert
        Debug.Assert(error is null);
    }

    #endregion
}