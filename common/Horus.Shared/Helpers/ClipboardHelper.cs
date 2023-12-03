using TextCopy;

namespace Horus.Shared.Helpers;

public static class ClipboardHelper
{
    #region Members

    private static bool _hasBeenUsed;

    #endregion

    #region Public methods

    public static void Copy(string value)
    {
        _hasBeenUsed = true;
        ClipboardService.SetText(value);
    }

    public static void Clear()
    {
        ClipboardService.SetText(string.Empty);
    }

    public static void EnsureCleared()
    {
        if (_hasBeenUsed)
        {
            Clear();
        }
    }

    #endregion
}