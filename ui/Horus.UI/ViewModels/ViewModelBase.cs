using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;

namespace Horus.UI.ViewModels;

public class ViewModelBase : ReactiveObject
{
    #region Protected methods

    protected void InvokeUi(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

    #endregion
}