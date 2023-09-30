using System.Text.RegularExpressions;
using WinPass.Shared.Helpers;

namespace WinPass.Tests.Tests;

public class PasswordHelperTests
{
    #region Constants

    private static readonly Regex RegAlphaLow = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex RegAlphaUp = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex RegADigit = new("[0-9]", RegexOptions.Compiled);
    private static readonly Regex RegSpecial = new("[^a-zA-Z0-9]", RegexOptions.Compiled);

    #endregion

    #region Tests

    [Fact]
    public void CalculateEntropy_ShouldReturn131ish()
    {
        var password = PasswordHelper.Generate(20,
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890`~!@#$%^&*()-=_+[{]}\\");
        Assert.True(131 - CalculateEntropy(password.Select(c => (char)c).ToArray()) < 1);
    }
    
    [Fact]
    public void CalculateEntropy_ShouldReturn66ish()
    {
        var password = PasswordHelper.Generate(20,
            "1234567890");
        Assert.True(66 - CalculateEntropy(password.Select(c => (char)c).ToArray()) < 1);
    }

    #endregion

    #region Private methods

    private static int CalculatePoolSize(char[] chars, bool actualSize = false)
    {
        if (actualSize)
        {
            HashSet<char> pool = new();
            foreach (var c in chars)
            {
                pool.Add(c);
            }

            return pool.Count;
        }

        var length = chars.Length;
        var hasDigit = false;
        var hasLower = false;
        var hasUpper = false;
        var hasSpecial = false;

        for (var i = 0; i < length; ++i)
        {
            if (hasDigit && hasLower && hasUpper && hasSpecial) break;

            if (RegAlphaLow.IsMatch(chars[i].ToString()))
            {
                hasLower = true;
                continue;
            }

            if (RegAlphaUp.IsMatch(chars[i].ToString()))
            {
                hasUpper = true;
                continue;
            }

            if (RegADigit.IsMatch(chars[i].ToString()))
            {
                hasDigit = true;
                continue;
            }

            if (RegSpecial.IsMatch(chars[i].ToString()))
            {
                hasSpecial = true;
                continue;
            }
        }

        return (hasDigit ? 10 : 0) +
               (hasLower ? 26 : 0) +
               (hasUpper ? 26 : 0) +
               (hasSpecial ? 32 : 0);
    }

    private static double CalculateEntropy(char[] chars, bool actualSize = false, int length = 0)
    {
        return (length == 0 ? chars.Length : length) * Math.Log2(CalculatePoolSize(chars, actualSize));
    }

    #endregion
}