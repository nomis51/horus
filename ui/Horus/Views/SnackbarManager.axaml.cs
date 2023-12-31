﻿using Avalonia.Interactivity;
using Horus.Enums;
using Horus.ViewModels;

namespace Horus.Views;

public partial class SnackbarManager : ViewBase<SnackbarManagerViewModel>
{
    #region Constructors

    public SnackbarManager()
    {
        InitializeComponent();
    }

    #endregion

    #region Public methods

    public void ShowSnackbar(string message, SnackbarSeverity severity = SnackbarSeverity.Accent, int duration = 3000)
    {
        Dispatch(vm => vm?.ShowSnackbar(message, severity, duration));
    }

    #endregion

    #region Private methods

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CloseSnackbar();
    }

    #endregion
}