﻿using Avalonia.Interactivity;
using Horus.UI.Abstractions;
using Horus.UI.ViewModels;

namespace Horus.UI.Views.Dialogs;

public partial class DestroyStoreDialog : DialogView<DestroyStoreDialogViewModel>
{
    #region Constructors

    public DestroyStoreDialog()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose(false);
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        OnClose(false);
    }

    private void ButtonFirstConfirm_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.PerformFirstConfirm();
    }

    private void ButtonSecondConfirm_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.PerformSecondConfirm();
    }

    private void ButtonDestroy_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.DestroyStore()) return;

            InvokeUi(() => OnClose(true));
        });
    }

    #endregion
}