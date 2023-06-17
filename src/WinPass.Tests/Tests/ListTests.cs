using System.Diagnostics;
using System.Text;
using WinPass.Shared.Models.Errors.Fs;

namespace WinPass.Tests.Tests;

public class ListTests : Test
{
    #region Members

    private readonly Cli _sut;
    private const string GpgId = "F76E20A0A9DF6C1F236D1DC5F8DFEDDF0640812A";
    private readonly string _storePath;
    private readonly string _gpgIdFilePath;

    #endregion

    #region Constructors

    public ListTests(StringBuilder stdout):base(stdout)
    {
        _sut = new Cli();
        _storePath = Environment.ExpandEnvironmentVariables("%USERPROFILE%/.password-store");
        _gpgIdFilePath = Path.Join(_storePath, ".gpg-id");
    }

    #endregion

    #region Tests

    public void ShouldListEntries()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        Directory.CreateDirectory(_storePath);
        File.WriteAllText(_gpgIdFilePath, GpgId);
        Directory.CreateDirectory(Path.Join(_storePath, "folder1"));
        Directory.CreateDirectory(Path.Join(_storePath, "folder2"));
        File.WriteAllText(Path.Join(_storePath, "password1.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "folder1", "password2.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "password3.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "folder2", "password4.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "folder2", "password5.gpg"), string.Empty);

        // Act
        _sut.Run(new[] { "list" });

        // Assert
        var strout = Stdout.ToString();
        var password1Index = strout.IndexOf("password1", StringComparison.Ordinal);
        var folder1Index = strout.IndexOf("folder1", StringComparison.Ordinal);
        var password2Index = strout.IndexOf("password2", StringComparison.Ordinal);
        var password3Index = strout.IndexOf("password3", StringComparison.Ordinal);
        var folder2Index = strout.IndexOf("folder2", StringComparison.Ordinal);
        var password4Index = strout.IndexOf("password4", StringComparison.Ordinal);
        var password5Index = strout.IndexOf("password5", StringComparison.Ordinal);

        Debug.Assert(password1Index != -1);
        Debug.Assert(password2Index != -1);
        Debug.Assert(password3Index != -1);
        Debug.Assert(password4Index != -1);
        Debug.Assert(password5Index != -1);
        Debug.Assert(folder1Index != -1);
        Debug.Assert(folder2Index != -1);
        Debug.Assert(folder1Index < password2Index);
        Debug.Assert(folder2Index < password4Index);
        Debug.Assert(folder2Index < password5Index);
    }
    
    public void ShouldListEntries_WithLs()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        Directory.CreateDirectory(_storePath);
        File.WriteAllText(_gpgIdFilePath, GpgId);
        Directory.CreateDirectory(Path.Join(_storePath, "folder1"));
        Directory.CreateDirectory(Path.Join(_storePath, "folder2"));
        File.WriteAllText(Path.Join(_storePath, "password1.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "folder1", "password2.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "password3.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "folder2", "password4.gpg"), string.Empty);
        File.WriteAllText(Path.Join(_storePath, "folder2", "password5.gpg"), string.Empty);

        // Act
        _sut.Run(new[] { "ls" });

        // Assert
        var strout = Stdout.ToString();
        var password1Index = strout.IndexOf("password1", StringComparison.Ordinal);
        var folder1Index = strout.IndexOf("folder1", StringComparison.Ordinal);
        var password2Index = strout.IndexOf("password2", StringComparison.Ordinal);
        var password3Index = strout.IndexOf("password3", StringComparison.Ordinal);
        var folder2Index = strout.IndexOf("folder2", StringComparison.Ordinal);
        var password4Index = strout.IndexOf("password4", StringComparison.Ordinal);
        var password5Index = strout.IndexOf("password5", StringComparison.Ordinal);

        Debug.Assert(password1Index != -1);
        Debug.Assert(password2Index != -1);
        Debug.Assert(password3Index != -1);
        Debug.Assert(password4Index != -1);
        Debug.Assert(password5Index != -1);
        Debug.Assert(folder1Index != -1);
        Debug.Assert(folder2Index != -1);
        Debug.Assert(folder1Index < password2Index);
        Debug.Assert(folder2Index < password4Index);
        Debug.Assert(folder2Index < password5Index);
    }
    
    public void ShouldErrorWithStoreNotInitialized()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        Directory.CreateDirectory(_storePath);

        // Act
        _sut.Run(new[] { "list" });

        // Assert
        Debug.Assert(Stdout.ToString().Contains(new FsStoreNotInitializedError().Message));
    }
    
    public void ShouldDisplayNothing_StoreEmpty()
    {
        // Arrange
        Stdout.Clear();
        if (Directory.Exists(_storePath)) Directory.Delete(_storePath, true);
        Directory.CreateDirectory(_storePath);
        File.WriteAllText(_gpgIdFilePath, GpgId);

        // Act
        _sut.Run(new[] { "list" });

        // Assert
       Debug.Assert(string.IsNullOrWhiteSpace(Stdout.ToString()));
    }

    #endregion
}