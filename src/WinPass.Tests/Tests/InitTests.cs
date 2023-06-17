using System.Diagnostics;
using System.Text;
using WinPass.Shared.Models.Errors.Fs;
using WinPass.Shared.Models.Errors.Gpg;

namespace WinPass.Tests.Tests;

public class InitTests : Test
{
    #region Members

    private readonly Cli _sut;
    private const string GpgId = "F76E20A0A9DF6C1F236D1DC5F8DFEDDF0640812A";
    private readonly string _storePath;
    private readonly string _gpgIdFilePath;

    #endregion

    #region Constructors

    public InitTests(StringBuilder stdout) : base(stdout)
    {
        _sut = new Cli();
        _storePath = Environment.ExpandEnvironmentVariables("%USERPROFILE%/.password-store");
        _gpgIdFilePath = Path.Join(_storePath, ".gpg-id");
    }

    #endregion

    #region Tests

    public void ShouldInitializeStore()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath))
        {
            Directory.Delete(_storePath, true);
        }

        // Act
        _sut.Run(new[] { "init", GpgId });

        // Assert
        Debug.Assert(Directory.Exists(_storePath));
        Debug.Assert(File.Exists(_gpgIdFilePath));
        Debug.Assert(GpgId.Equals(File.ReadAllText(_gpgIdFilePath)));
        Debug.Assert(Stdout.ToString().Contains("Store initialized"));
    }


    public void ShouldErrorWithStoreAlreadyInitialized()
    {
        // Arrange
        Stdout.Clear();
        if (!Directory.Exists(_storePath))
        {
            Directory.CreateDirectory(_storePath);
            File.WriteAllText(_gpgIdFilePath, GpgId);
        }

        // Act
        _sut.Run(new[] { "init", GpgId });

        // Assert
        Debug.Assert(Stdout.ToString().Contains(new FsStoreAlreadyInitializedError().Message));
    }


    public void ShouldErrorWithGpgIdMissing()
    {
        // Arrange
        Stdout.Clear();

        // Act
        _sut.Run(new[] { "init" });

        // Assert
        Debug.Assert(Stdout.ToString().Contains("GPG key ID argument required"));
    }


    public void ShouldErrorWithGpgIdNotFound()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath))
        {
            Directory.Delete(_storePath, true);
        }

        // Act
        _sut.Run(new[] { "init", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" });

        // Assert
        Debug.Assert(Stdout.ToString().Contains(new GpgKeyNotFoundError().Message));
    }


    public void ShouldErrorInvalidGpgId()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath))
        {
            Directory.Delete(_storePath, true);
        }

        // Act
        _sut.Run(new[] { "init", "ABC" });

        // Assert
        Debug.Assert(Stdout.ToString().Contains("Invalid GPG key ID provided"));
    }

    #endregion
}