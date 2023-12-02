namespace Horus.UI.Services;

public class SpinnerOverlayService
{
    #region Singleton

    private static readonly object LockInstance = new();
#pragma warning disable CS8618
    private static SpinnerOverlayService _instance;
#pragma warning restore CS8618

    public static SpinnerOverlayService Instance
    {
        get
        {
            lock (LockInstance)
            {
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                _instance ??= new SpinnerOverlayService();
            }

            return _instance;
        }
    }

    #endregion

    #region Events

    public delegate void ShowEvent(string message);

    public event ShowEvent? OnShow;

    public delegate void HideEvent();

    public event HideEvent? OnHide;

    #endregion

    #region Public methods

    public void Show(string message = "")
    {
        OnShow?.Invoke(message);
    }

    public void Hide()
    {
        OnHide?.Invoke();
    }

    #endregion
}