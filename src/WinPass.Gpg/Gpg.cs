using WinPass.Shared.Models.Abstractions;

namespace WinPass.Gpg;

public class Gpg
{
    #region Members

    private readonly string _keyId;

    #endregion

    #region Constructors

    public Gpg(string keyId)
    {
        _keyId = keyId;
    }

    #endregion

    #region Public methods

    public static bool Verify()
    {
        return false;
    }

    public ResultStruct<byte, Error?> Encrypt( string filePath, string value)
    {
        return new ResultStruct<byte, Error?>(0);
    }

    public Result<string, Error?> Decrypt(string filePath)
    {
        return new Result<string, Error?>("");
    }

    public ResultStruct<bool, Error?> IsValid()
    {
        
    }

    #endregion
}