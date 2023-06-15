namespace WinPass.Shared.Extensions;

public static class StringExtensions
{
    #region Public methods

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