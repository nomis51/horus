using System.Text;

namespace WinPass.Tests;

public class InitTests
{
    #region Members

    private readonly Cli _sut;
    private readonly StringBuilder _stdout;
    private const string GpgId = "8560851CF72DB8F78D1E32B8E844BEA9C7BD333B";
    private readonly string _storePath;
    private readonly string _gpgIdFilePath;

    #endregion

    #region Constructors

    public InitTests()
    {
        _sut = new Cli();
        _stdout = new StringBuilder();
        _storePath = Environment.ExpandEnvironmentVariables("%USERPROFILE%/.password-store");
        _gpgIdFilePath = Path.Join(_storePath, ".gpg-id");
        Console.SetOut(new StringWriter(_stdout));
    }

    #endregion

    #region Tests

    [Fact]
    public void ShouldInitializeStore()
    {
        lock (this)
        {
            // Arrange
            _stdout.Clear();
            if (Directory.Exists(_storePath))
            {
                Directory.Delete(_storePath, true);
            }

            // Act
            _sut.Run(new[] { "init", GpgId });

            // Assert
            Assert.True(Directory.Exists(_storePath));
            Assert.True(File.Exists(_gpgIdFilePath));
            Assert.Equal(GpgId, File.ReadAllText(_gpgIdFilePath));
            Assert.Contains("Store initialized", _stdout.ToString());
        }
    }

    [Fact]
    public void ShouldErrorWithStoreAlreadyInitialized()
    {
        lock (this)
        {
            // Arrange
            _stdout.Clear();
            if (!Directory.Exists(_storePath))
            {
                Directory.CreateDirectory(_storePath);
                File.WriteAllText(_gpgIdFilePath, GpgId);
            }

            // Act
            _sut.Run(new[] { "init", GpgId });

            // Assert
            Assert.True(Directory.Exists(_storePath));
            Assert.True(File.Exists(_gpgIdFilePath));
            Assert.Equal(GpgId, File.ReadAllText(_gpgIdFilePath));
            Assert.Contains("Store already initialized", _stdout.ToString());
        }
    }

    [Fact]
    public void ShouldErrorWithGpgIdMissing()
    {
        lock (this)
        {
            // Arrange
            _stdout.Clear();

            // Act
            _sut.Run(new[] { "init" });

            // Assert
            Assert.Contains("GPG key ID argument required", _stdout.ToString());
        }
    }

    [Fact]
    public void ShouldErrorWithGpgIdNotFound()
    {
        lock (this)
        {
            // Arrange
            _stdout.Clear();
            if (Directory.Exists(_storePath))
            {
                Directory.Delete(_storePath, true);
            }

            // Act
            _sut.Run(new[] { "init", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" });

            // Assert
            Assert.Contains("GPG key not found", _stdout.ToString());
        }
    }

    [Fact]
    public void ShouldErrorInvalidGpgId()
    {
        lock (this)
        {
            // Arrange
            _stdout.Clear();
            if (Directory.Exists(_storePath))
            {
                Directory.Delete(_storePath, true);
            }

            // Act
            _sut.Run(new[] { "init", "ABC" });

            // Assert
            Assert.Contains("Invalid GPG key ID provided", _stdout.ToString());
        }
    }

    #endregion
}