using System.Runtime.InteropServices;
using System.Text;

namespace WinPass.Core.WinApi;

public static class User32
{
    #region Constants

    private const uint CfUnicodeText = 13; 

    #endregion
    
    #region Imports

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    #endregion

    #region Public methods

    public static void ClearClipboard()
    {
        OpenClipboard(IntPtr.Zero);
        EmptyClipboard();
        CloseClipboard();
    }

    public static void SetClipboard(string value)
    {
        if (!value.EndsWith('\0'))
        {
            value += '\0';
        }

        var strBytes = Encoding.Unicode.GetBytes(value);
        var hglobal = Marshal.AllocHGlobal(strBytes.Length);
        Marshal.Copy(strBytes, 0, hglobal, strBytes.Length);

        OpenClipboard(IntPtr.Zero);
        EmptyClipboard();
        SetClipboardData(CfUnicodeText, hglobal);
        CloseClipboard();
        Marshal.FreeHGlobal(hglobal);
    }

    #endregion
}