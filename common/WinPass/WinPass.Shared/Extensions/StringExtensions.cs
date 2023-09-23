using System.Text.RegularExpressions;

namespace WinPass.Shared.Extensions;

public static class StringExtensions
{
    #region Constants

    private static readonly Regex RegBase64 = new("([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    #endregion

    #region Public methods

    public static bool IsBase64(this string value)
    {
        var trimmedValue = value.TrimEnd('\n', '\r').Trim();
        var match = RegBase64.Match(trimmedValue);
        return match is { Success: true, Index: 0 } && match.Length == trimmedValue.Length;
    }

    public static string ToBase64(this string value)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    public static string FromBase64(this string value)
    {
        var bytes = Convert.FromBase64String(value);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    #endregion
}