namespace WinPass.UI.Extensions;

public static class StringExtensions
{
    #region Public methods

    public static string Capitalize(this string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Length == 1) return value.ToUpper();

        return value[..1].ToUpper() + value[1..];
    }

    #endregion
}