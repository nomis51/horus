using Avalonia.Controls;

namespace WinPass.UI.Extensions;

public static class ObjectExtensions
{
    #region Public methods

    public static T GetTag<T>(this object sender)
    {
        return (T)(sender as Control)!.Tag!;
    }

    #endregion
}