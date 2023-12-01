using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DynamicData;
using Horus.Core.Services;
using Horus.Shared.Enums;
using Horus.Shared.Models.Data;
using Horus.UI.Enums;
using Horus.UI.Models;
using Horus.UI.Services;
using ReactiveUI;

namespace Horus.UI.ViewModels;

public class EntryFormViewModel : ViewModelBase
{
    #region Props

    private string _entryName = string.Empty;

    public string EntryName
    {
        get => _entryName;
        set
        {
            this.RaiseAndSetIfChanged(ref _entryName, value);
            this.RaisePropertyChanged(nameof(HasEntryName));
            this.RaisePropertyChanged(nameof(HasEntryNameAndNotEditingIt));
        }
    }

    public bool HasEntryName => !string.IsNullOrWhiteSpace(EntryName);

    public bool HasEntryNameAndNotEditingIt => HasEntryName && !IsEditingName;

    public ObservableCollection<MetadataModel> Metadatas { get; } = new();
    public ObservableCollection<MetadataModel> InternalMetadatas { get; } = new();
    private bool _areMetadatasRevealed;

    public bool AreMetadatasRevealed
    {
        get => _areMetadatasRevealed;
        private set
        {
            this.RaiseAndSetIfChanged(ref _areMetadatasRevealed, value);
            this.RaisePropertyChanged(nameof(HasMetadatas));
        }
    }

    public bool HasMetadatas => Metadatas.Any() || InternalMetadatas.Any();
    public bool HasNormalMetadatas => Metadatas.Any();
    public int[] MetadatasPlaceholders { get; private set; } = { 0, 0, 0 };
    private string _password = string.Empty;

    public string Password
    {
        get => _password;
        set
        {
            this.RaiseAndSetIfChanged(ref _password, value);
            this.RaisePropertyChanged(nameof(IsPasswordValid));
        }
    }

    private string _confirmPassword = string.Empty;

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            this.RaiseAndSetIfChanged(ref _confirmPassword, value);
            this.RaisePropertyChanged(nameof(IsPasswordValid));
        }
    }

    public bool IsPasswordValid => Password == ConfirmPassword && !string.IsNullOrEmpty(Password);
    public bool CanSavePassword => IsPasswordValid && !IsLoading;

    private bool _isEditingPassword;

    public bool IsEditingPassword
    {
        get => _isEditingPassword;
        private set => this.RaiseAndSetIfChanged(ref _isEditingPassword, value);
    }

    private double _passwordLength = 12;

    public double PasswordLength
    {
        get => _passwordLength;
        set => this.RaiseAndSetIfChanged(ref _passwordLength, value);
    }

    private bool _isGeneratingPassword;

    public bool IsGeneratingPassword
    {
        get => _isGeneratingPassword;
        set => this.RaiseAndSetIfChanged(ref _isGeneratingPassword, IsEditingPassword && value);
    }

    private bool _isPasswordVisible;

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => this.RaiseAndSetIfChanged(ref _isPasswordVisible, IsEditingPassword && value);
    }

    private string _customPasswordAlphabet = string.Empty;

    public string CustomPasswordAlphabet
    {
        get => _customPasswordAlphabet;
        set => this.RaiseAndSetIfChanged(ref _customPasswordAlphabet, value);
    }

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private bool _isEditingName;

    public bool IsEditingName
    {
        get => _isEditingName;
        set => this.RaiseAndSetIfChanged(ref _isEditingName, value);
    }

    private string _newEntryName = string.Empty;

    public string NewEntryName
    {
        get => _newEntryName;
        set { this.RaiseAndSetIfChanged(ref _newEntryName, value); }
    }

    #endregion

    #region Constructors

    public EntryFormViewModel()
    {
        ResetPassword();
    }

    #endregion

    #region Public methods

    public bool SaveNewEntryName()
    {
        IsLoading = true;
        var result = AppService.Instance.RenameStoreEntry(EntryName, NewEntryName);
        IsLoading = false;

        if (result.HasError)
        {
            SnackbarService.Instance.Show("Failed to rename entry", SnackbarSeverity.Error, 5000);
            return false;
        }

        IsEditingName = false;
        SetEntryItem(NewEntryName);
        this.RaisePropertyChanged(nameof(HasEntryNameAndNotEditingIt));
        SnackbarService.Instance.Show("Entry renamed", SnackbarSeverity.Success);
        return true;
    }

    public void EditName()
    {
        NewEntryName = EntryName;
        IsEditingName = true;
        this.RaisePropertyChanged(nameof(HasEntryNameAndNotEditingIt));
    }

    public void CancelEditName()
    {
        IsEditingName = false;
        this.RaisePropertyChanged(nameof(HasEntryNameAndNotEditingIt));
    }

    public void CopyOldPassword()
    {
        IsLoading = true;
        var (_, error) = AppService.Instance.GetPassword(EntryName);
        IsLoading = false;

        if (error is null) return;

        SnackbarService.Instance.Show("Unable to copy password", SnackbarSeverity.Warning);
    }

    public void GeneratePassword()
    {
        if (!IsEditingPassword) return;

        if (!IsGeneratingPassword)
        {
            IsGeneratingPassword = true;
        }

        var (password, error) = AppService.Instance.GenerateNewPassword(Convert.ToInt32(PasswordLength), CustomPasswordAlphabet);
        if (error is not null)
        {
            SnackbarService.Instance.Show("Unable generate password", SnackbarSeverity.Warning);
            return;
        }

        IsPasswordVisible = true;
        Password = password!.ValueAsString;
        ConfirmPassword = password.ValueAsString;
        password.Dispose();
    }

    public void SavePassword()
    {
        IsLoading = true;
        var result = AppService.Instance.EditPassword(EntryName, new Password(Password));
        IsLoading = false;

        if (result.HasError)
        {
            SnackbarService.Instance.Show("Unable to save password", SnackbarSeverity.Error, 5000);
            return;
        }

        SnackbarService.Instance.Show("Password saved", SnackbarSeverity.Success);
        CancelPassword();
    }

    public void EditPassword()
    {
        ClearPassword();
        IsEditingPassword = true;
    }

    public void CancelPassword()
    {
        IsEditingPassword = false;
        IsGeneratingPassword = false;
        IsPasswordVisible = false;
        ResetPassword();
    }

    public void SaveMetadatas()
    {
        IsLoading = true;
        var result = AppService.Instance.EditMetadatas(
            EntryName,
            new MetadataCollection(
                EntryName,
                InternalMetadatas.ToList()
                    .Concat(Metadatas.ToList())
                    .Select(e => new Metadata(e.Key, e.Value, e.Type))
                    .Where(m => !string.IsNullOrWhiteSpace(m.Key) || !string.IsNullOrWhiteSpace(m.Value))
                    .ToList()
            )
        );
        IsLoading = false;

        if (result.HasError)
        {
            SnackbarService.Instance.Show("Unable to save metadata", SnackbarSeverity.Error, 5000);
            return;
        }

        SnackbarService.Instance.Show("Metadata saved", SnackbarSeverity.Success);
        CancelMetadatas();
    }

    public void RemoveMetadata(Guid id)
    {
        var index = Metadatas.Select(m => m.Id).IndexOf(id);
        if (index == -1) return;

        Metadatas.RemoveAt(index);
        this.RaisePropertyChanged(nameof(HasNormalMetadatas));
    }

    public void SetEntryItem(string name)
    {
        EntryName = name;
        IsEditingPassword = false;
        AreMetadatasRevealed = false;
    }

    public void RetrieveMetadatas()
    {
        IsLoading = true;
        var (metadatas, error) = AppService.Instance.GetMetadatas(EntryName);
        IsLoading = false;

        if (error is not null) return;

        InvokeUi(() =>
        {
            foreach (var metadata in metadatas)
            {
                var item = new MetadataModel
                {
                    Key = metadata.Key,
                    Value = metadata.Value,
                    Type = metadata.Type,
                };

                if (metadata.Type == MetadataType.Internal)
                {
                    InternalMetadatas.Add(item);
                }
                else if (metadata.Type == MetadataType.Normal)
                {
                    Metadatas.Add(item);
                }
            }
        });

        AreMetadatasRevealed = true;
        this.RaisePropertyChanged(nameof(HasNormalMetadatas));
    }

    public void AddMetadata()
    {
        Metadatas.Add(new MetadataModel());
        this.RaisePropertyChanged(nameof(HasNormalMetadatas));
    }

    public void CancelMetadatas()
    {
        Metadatas.Clear();
        InternalMetadatas.Clear();
        AreMetadatasRevealed = false;
        this.RaisePropertyChanged(nameof(HasNormalMetadatas));
    }

    #endregion

    #region Private methods

    private void ClearPassword()
    {
        Password = string.Empty;
        ConfirmPassword = string.Empty;
    }

    private void ResetPassword()
    {
        Password = "************";
        ConfirmPassword = "************";
    }

    #endregion
}