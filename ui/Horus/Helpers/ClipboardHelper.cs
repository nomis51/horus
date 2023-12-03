using System;
using System.Threading;
using Horus.Shared.Extensions;
using TextCopy;

namespace Horus.Helpers;

public static class ClipboardHelper
{
    #region Constants

    private const int NbClipboardPollutionIterations = 50;

    #endregion

    #region Public methods

    public static void SafeClear(int delay)
    {
        Thread.Sleep(1000 * delay);

        // To polluate any clipboard history tracker
        for (var i = 0; i < NbClipboardPollutionIterations; ++i)
        {
            Copy(Guid.NewGuid().ToString().ToBase64());
        }

        Clear();
    }

    #endregion

    #region Private methods

    private static void Copy(string value)
    {
        ClipboardService.SetText(value);
    }

    private static void Clear()
    {
        ClipboardService.SetText(string.Empty);
    }

    #endregion
}