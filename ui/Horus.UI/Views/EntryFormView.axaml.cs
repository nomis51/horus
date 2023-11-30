using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Horus.UI.Extensions;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public partial class EntryFormView : ViewBase<EntryFormViewModel>
{
    #region Events

    public delegate void DeleteEntryEvent(string name);

    public event DeleteEntryEvent? DeleteEntry;

    public delegate void DuplicateEntryEvent(string name);

    public event DuplicateEntryEvent? DuplicateEntry;
    
    public delegate void EntryRenamedEntryEvent();

    public event EntryRenamedEntryEvent? EntryRenamedEntry;

    #endregion

    #region Constructors

    public EntryFormView()
    {
        InitializeComponent();
    }

    #endregion

    #region Public methods

    public void SetEntryItem(string name, bool isNew = false)
    {
        ViewModel?.SetEntryItem(name);
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
        SliderPasswordLength.Value = Math.Round(SliderPasswordLength.Value);
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
        DeleteEntry?.Invoke(ViewModel!.EntryName);
    }

    private void ButtonCloneEntry_OnClick(object? sender, RoutedEventArgs e)
    {
        DuplicateEntry?.Invoke(ViewModel!.EntryName);
    }

    private void ButtonEntryName_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
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

    #endregion
}