using Horus.UI.Enums;

namespace Horus.UI.Services;

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

    #endregion

    #region Public methods

    public void Show(DialogType dialogType, object? data = null)
    {
        OnShow?.Invoke(dialogType, data);
    }

    #endregion
}