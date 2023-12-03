using Avalonia.Controls;

namespace Horus.Extensions;

public static class ObjectExtensions
{
    #region Public methods

    public static T GetTag<T>(this object sender)
    {
        return (T)(sender as Control)!.Tag!;
    }

    #endregion
}