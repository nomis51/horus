using Horus.UI.ViewModels;
using Horus.UI.Views;

namespace Horus.UI.Abstractions;

public class DialogView<T> : ViewBase<T>
    where T : ViewModelBase
{
    #region Events

    public delegate void CloseEvent(object? data = null);

    public event CloseEvent? Close;

    #endregion

    #region Protected methods
    
    protected void OnClose(object? data = null)
    {
        Close?.Invoke(data);
    }

    #endregion
}