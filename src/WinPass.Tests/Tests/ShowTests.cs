using System.Diagnostics;
using System.Text;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Tests.Tests;

public class ShowTests : Test
{
    #region Members

    private readonly Cli _sut;
    private const string GpgId = "F76E20A0A9DF6C1F236D1DC5F8DFEDDF0640812A";
    private readonly string _storePath;
    private readonly string _gpgIdFilePath;

    #endregion

    #region Constructors

    public ShowTests(StringBuilder stdout):base(stdout)
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
        Directory.CreateDirectory(_storePath);
        File.WriteAllText(_gpgIdFilePath, GpgId);

        // Act
        _sut.Run(new[] { "show" });

        // Assert
       Debug.Assert(Stdout.ToString().Contains("password name argument required"));
    }
    
    public void ShouldErrorWithEntryNotFound()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        Directory.CreateDirectory(_storePath);
        File.WriteAllText(_gpgIdFilePath, GpgId);

        // Act
        _sut.Run(new[] { "show", "notfound" });

        // Assert
        Debug.Assert(Stdout.ToString().Contains(new FsEntryNotFoundError().Message));
    }

    #endregion
}