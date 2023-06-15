namespace WinPass.Tests;

public class InitTests
{
    #region Members

    private readonly Cli _sut;

    #endregion

    #region Constructors

    public InitTests()
    {
        _sut = new Cli();
    }

    #endregion

    #region Tests

    [Fact]
    public void ShouldInitialiazeStore()
    {
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