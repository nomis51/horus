using TextCopy;

namespace WinPass.Shared.Helpers;

public static class ClipboardHelper
{
    #region Public methods

    public static void Copy(string value)
    {
        ClipboardService.SetText(value);
    }

    public static void Clear()
    {
        ClipboardService.SetText(string.Empty);
    }

    #endregion
}