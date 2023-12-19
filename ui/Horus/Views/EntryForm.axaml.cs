using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Horus.Enums;
using Horus.Extensions;
using Horus.Services;
using Horus.ViewModels;
using Serilog;

namespace Horus.Views;

public partial class EntryForm : ViewBase<EntryFormViewModel>
{
    #region Events

    public delegate void EntryRenamedEntryEvent();

    public event EntryRenamedEntryEvent? EntryRenamedEntry;

    public delegate void EntryClosedEvent();

    public event EntryClosedEvent? EntryClosed;

    #endregion

    #region Constructors

    public EntryForm()
    {
        InitializeComponent();
    }

    #endregion

    #region Public methods

    public void SetEntryItem(string name, bool isNew = false)
    {
        ViewModel?.SetEntryItem(name);
    }

    public void WindowResized(double height)
    {
        Dispatch(vm => { vm!.WindowHeight = height; });
    }

    #endregion

    #region Private methods

    private void ButtonRevealMetadatas_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.RetrieveMetadatas());
    }

    private void ButtonAddMetadata_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.AddMetadata();
    }

    private void ButtonRemoveMetadata_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RemoveMetadata(sender!.GetTag<Guid>());
    }

    private void ButtonCancelMetadatas_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CancelMetadatas();
    }

    private void ButtonSaveMetadatas_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.SaveMetadatas());
    }

    private void ButtonEditPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.EditPassword();
    }

    private void ButtonCancelPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CancelPassword();
    }

    private void ButtonSavePassword_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.SavePassword());
    }

    private void SliderPasswordLength_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        SliderPasswordLength.Value = Math.Round((double)SliderPasswordLength.Value);
        ViewModel?.GeneratePassword();
    }

    private void ButtonGenerateNewPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.GeneratePassword();
    }

    private void TextBoxCustomPasswordAlphabet_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel?.GeneratePassword();
    }

    private void ButtonCopyOldPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm => vm?.CopyOldPassword());
    }

    private void ButtonDeleteEntry_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogService.Instance.Show(DialogType.DeleteEntry, ViewModel!.EntryName);
    }

    private void ButtonDuplicateEntry_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogService.Instance.Show(DialogType.DuplicateEntry, ViewModel!.EntryName);
    }

    private void ButtonCancelNewName_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CancelEditName();
    }

    private void ButtonSaveNewName_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.SaveNewEntryName()) return;

            EntryRenamedEntry?.Invoke();
        });
    }

    private void ButtonEntryName_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel!.IsEditingPassword || ViewModel!.AreMetadatasRevealed) return;
        ViewModel?.EditName();
    }

    private void TextBox_OnCopyingToClipboard(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Dispatch(vm => vm?.CopyOldPassword());
    }

    private void ButtonAttachFile_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        Dispatch(vm => vm?.SelectFile(topLevel));
    }


    private void TextBoxFile_OnCopyingToClipboard(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var key = sender!.GetTag<string>();

        Dispatch(vm => vm?.SaveFile(topLevel, key));
    }

    private void TextBoxUrl_OnCuttingToClipboard(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        Dispatch(vm => vm?.OpenUrl());
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        EntryClosed?.Invoke();
        ViewModel!.EntryName = string.Empty;
    }

    #endregion
}