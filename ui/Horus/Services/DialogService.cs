using Horus.Enums;

namespace Horus.Services;

public class DialogService
{
    #region Singleton

    private static readonly object LockInstance = new();
#pragma warning disable CS8618
    private static DialogService _instance;
#pragma warning restore CS8618

    public static DialogService Instance
    {
        get
        {
            lock (LockInstance)
            {
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                _instance ??= new DialogService();
            }

            return _instance;
        }
    }

    #endregion

    #region Events

    public delegate void ShowEvent(DialogType dialogType, object? data = null);

    public event ShowEvent? OnShow;

    public delegate void CloseEvent(DialogType dialogType, object? data = null);

    public event CloseEvent? OnClose;

    #endregion

    #region Public methods

    public void Show(DialogType dialogType, object? data = null)
    {
        OnShow?.Invoke(dialogType, data);
    }

    public void NotifyClose(DialogType dialogType, object? data = null)
    {
        OnClose?.Invoke(dialogType, data);
    }

    #endregion
}