using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Horus.Shared.Helpers;

public static class PasswordHelper
{
    #region Constants

    private const int MaxBestPasswordEntropyGenerationTries = 10;
    private const int DefaultLength = 20;

    private static readonly char[] Letters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] CapitalLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] Numbers = "1234567890".ToCharArray();
    private static readonly char[] BasicSymbols = "`~!@#$%^&*()-=_+[{]}\\/".ToCharArray();

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

        var chars = GetCharacters(customAlphabet);

        var data = new byte[4 * length];
        using var crypto = RandomNumberGenerator.Create();

        var tries = 0;
        var result = new char[length];
        while (tries < MaxBestPasswordEntropyGenerationTries)
        {
            ++tries;
            crypto.GetBytes(data);

            var bestEntropy = Math.Floor(CalculateEntropy(chars, true, length));
            for (var i = 0; i < length; ++i)
            {
                var random = BitConverter.ToUInt32(data, i * 4);
                var index = random % chars.Length;

                result[i] = chars[index];
            }

            var entropy = CalculateEntropy(result);
            if (entropy < bestEntropy) continue;

            break;
        }

        return result.Select(c => (byte)c).ToArray();
    }

    #endregion

    #region Private methods

    private static char[] GetCharacters(string alphabetQuery = "")
    {
        if (string.IsNullOrWhiteSpace(alphabetQuery))
        {
            return Letters.Concat(CapitalLetters)
                .Concat(Numbers)
                .Concat(BasicSymbols)
                .ToArray();
        }

        List<char[]> result = new();

        if (alphabetQuery.Contains("a-Z") || alphabetQuery.Contains("A-z"))
        {
            result.Add(Letters);
            result.Add(CapitalLetters);
            alphabetQuery = alphabetQuery.Replace("a-Z", string.Empty);
            alphabetQuery = alphabetQuery.Replace("A-z", string.Empty);
        }
        else
        {
            if (alphabetQuery.Contains("a-z"))
            {
                result.Add(Letters);
                alphabetQuery = alphabetQuery.Replace("a-z", string.Empty);
            }

            if (alphabetQuery.Contains("A-Z"))
            {
                result.Add(CapitalLetters);
                alphabetQuery = alphabetQuery.Replace("A-Z", string.Empty);
            }
        }

        if (alphabetQuery.Contains("0-9"))
        {
            result.Add(Numbers);
            alphabetQuery = alphabetQuery.Replace("0-9", string.Empty);
        }

        if (alphabetQuery.Contains("*-*"))
        {
            result.Add(BasicSymbols);
            alphabetQuery = alphabetQuery.Replace("*-*", string.Empty);
        }

        return result.SelectMany(r => r)
            .Concat(alphabetQuery.ToCharArray())
            .ToArray();
    }

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