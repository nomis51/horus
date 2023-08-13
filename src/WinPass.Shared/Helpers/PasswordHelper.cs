using System.Security.Cryptography;
using System.Text;

namespace WinPass.Shared.Helpers;

public static class PasswordHelper
{
    #region Constants

    private const int DefaultLength = 20;

    private static readonly char[] DefaultCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!\"/$%?&*()_+=-@\\#|.<>{}[]".ToCharArray();

    #endregion

    #region Public methods

    public static byte[] Generate(int length = DefaultLength, string customAlphabet = "")
    {
        if (length <= 0)
        {
            length = DefaultLength;
        }

        var chars = string.IsNullOrWhiteSpace(customAlphabet) ? DefaultCharacters : customAlphabet.ToCharArray();

        var data = new byte[4 * length];
        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }

        var result = new byte[length];
        for (var i = 0; i < length; ++i)
        {
            var random = BitConverter.ToUInt32(data, i * 4);
            var index = random % chars.Length;

            result[i] = (byte)chars[index];
        }

        return result;
    }

    #endregion
}