using System;
using Avalonia.Threading;
using ReactiveUI;

namespace Horus.ViewModels;

public class ViewModelBase : ReactiveObject
{
    #region Protected methods

    protected void InvokeUi(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }

    #endregion
}