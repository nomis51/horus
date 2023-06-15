using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace WinPass.Tests;

public class InitTests
{
    #region Members

    private readonly Cli _sut;
    private readonly StringBuilder _stdout;
    private const string GpgId = "F7153DF773DBD9AD";

    #endregion

    #region Constructors

    public InitTests()
    {
        _sut = new Cli();
        _stdout = new StringBuilder();
        Console.SetOut(new StringWriter(_stdout));
    }

    #endregion

    #region Tests

    [Fact]
    public void ShouldInitializeStore()
    {
        // Arrange
        var method = _sut.GetType().GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        
        // Act
        method!.Invoke(_sut, new object[] { GpgId });

        // Assert
        var g = 0;
    }

    [Fact]
    public void ShouldErrorWithStoreAlreadyInitialized()
    {
    }

    [Fact]
    public void ShouldErrorWithGpgIdMissing()
    {
    }

    #endregion
}