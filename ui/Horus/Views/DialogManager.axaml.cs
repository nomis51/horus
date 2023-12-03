using Horus.Enums;
using Horus.ViewModels;

namespace Horus.Views;

public partial class DialogManager : ViewBase<DialogManagerViewModel>
{
    #region Events

    public delegate void CloseEvent(DialogType dialogType, object? data = null);

    public event CloseEvent? Close;

    #endregion

    #region Constructors

    public DialogManager()
    {
        InitializeComponent();
    }

    #endregion

    #region Public methods

    public void ShowDialog(DialogType type, object? data = null)
    {
        Dispatch(vm => vm?.ShowDialog(type, data));
    }

    #endregion

    #region Private methods

    private void Dialog_OnClose(object? data = null)
    {
        Dispatch(vm => vm?.CloseDialog());
        Close?.Invoke(ViewModel!.DialogType, data);
    }

    #endregion
}