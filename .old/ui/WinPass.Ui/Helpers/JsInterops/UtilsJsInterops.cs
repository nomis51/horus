using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace WinPass.UI.Helpers.JsInterops;

public static class UtilsJsInterops
{
    #region Constants

    private const string BaseName = "window.winpass.utils";
    private const string CopyToClipboardName = $"{BaseName}.copyToClipboard";

    #endregion

    #region Public methods

    public static async Task CopyToClipboard(this IJSRuntime runtime, string value)
    {
        await runtime.InvokeVoidAsync(CopyToClipboardName, value);
    }

    public static async Task OpenUrl(this IJSRuntime runtime, string url)
    {
        await runtime.InvokeVoidAsync("open", url, "_blank");
    }

    #endregion
}