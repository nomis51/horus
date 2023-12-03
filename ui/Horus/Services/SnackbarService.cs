using Horus.Enums;

namespace Horus.Services;

public class SnackbarService
{
    #region Singleton

    private static readonly object LockInstance = new();
#pragma warning disable CS8618
    private static SnackbarService _instance;
#pragma warning restore CS8618

    public static SnackbarService Instance
    {
        get
        {
            lock (LockInstance)
            {
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                _instance ??= new SnackbarService();
            }

            return _instance;
        }
    }

    #endregion

    #region Events

    public delegate void ShowEvent(string message, SnackbarSeverity severity = SnackbarSeverity.Accent, int duration = 3000);

    public event ShowEvent? OnShow;

    #endregion
    
    #region Public methods

    public void Show(string message, SnackbarSeverity severity = SnackbarSeverity.Accent, int duration = 3000)
    {
        OnShow?.Invoke(message, severity, duration);
    }
    
    #endregion
}