using Horus.Enums;
using Horus.Services;
using Horus.ViewModels;
using Horus.Views;

namespace Horus.Abstractions;

public class DialogView<T> : ViewBase<T>
    where T : ViewModelBase
{
    #region Events

    public delegate void CloseEvent(object? data = null);

    public event CloseEvent? Close;

    #endregion

    #region Members

    private readonly DialogType _type;

    #endregion

    #region Constructors

    protected DialogView(DialogType type)
    {
        _type = type;
    }

    #endregion

    #region Protected methods

    protected void OnClose(object? data = null)
    {
        Close?.Invoke(data);
        DialogService.Instance.NotifyClose(_type, data);
    }

    #endregion
}