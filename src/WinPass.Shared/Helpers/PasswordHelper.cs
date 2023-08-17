using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WinPass.Shared.Helpers;

public static class PasswordHelper
{
    #region Constants

    private const int DefaultLength = 20;
    private const double MinimumEntropy = 131d;

    private static readonly char[] DefaultCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890`~!@#$%^&*()-=_+[{]}\\".ToCharArray();

    private static readonly Regex RegAlphaLow = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex RegAlphaUp = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex RegADigit = new("[0-9]", RegexOptions.Compiled);
    private static readonly Regex RegSpecial = new("[^a-zA-Z0-9]", RegexOptions.Compiled);

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

        while (true)
        {
            var result = new byte[length];
            for (var i = 0; i < length; ++i)
            {
                var random = BitConverter.ToUInt32(data, i * 4);
                var index = random % chars.Length;

                result[i] = (byte)chars[index];
            }

            var entropy = CalculateEntropy(result);
            if (entropy < MinimumEntropy) continue;

            return result;
        }
    }

    #endregion

    #region Private methods

    private static double CalculateEntropy(byte[] chars)
    {
        var length = chars.Length;
        var hasDigit = false;
        var hasLower = false;
        var hasUpper = false;
        var hasSpecial = false;

        for (var i = 0; i < length; ++i)
        {
            if (hasDigit && hasLower && hasUpper && hasSpecial) break;

            if (RegAlphaLow.IsMatch(((char)chars[i]).ToString()))
            {
                hasLower = true;
                continue;
            }

            if (RegAlphaUp.IsMatch(((char)chars[i]).ToString()))
            {
                hasUpper = true;
                continue;
            }

            if (RegADigit.IsMatch(((char)chars[i]).ToString()))
            {
                hasDigit = true;
                continue;
            }

            if (RegSpecial.IsMatch(((char)chars[i]).ToString()))
            {
                hasSpecial = true;
                continue;
            }
        }

        return length * Math.Log2(
            (hasDigit ? 10 : 0) +
            (hasLower ? 26 : 0) +
            (hasUpper ? 26 : 0) +
            (hasSpecial ? 32 : 0)
        );
    }

    #endregion
}