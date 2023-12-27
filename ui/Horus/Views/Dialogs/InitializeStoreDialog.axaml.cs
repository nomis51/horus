using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Horus.Abstractions;
using Horus.Enums;
using Horus.ViewModels;

namespace Horus.Views.Dialogs;

public partial class InitializeStoreDialog : DialogView<InitializeStoreDialogViewModel>
{
    #region Constructors

    public InitializeStoreDialog()
        : base(DialogType.InitializeStore)
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.Validate()) return;

            InvokeUi(() => OnClose());
        });
    }

    private void ButtonQuit_OnClick(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void ComboBoxGpgId_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ViewModel!.GpgId = ComboBoxGpgId.SelectedItem!.ToString()!;
    }

    #endregion
}