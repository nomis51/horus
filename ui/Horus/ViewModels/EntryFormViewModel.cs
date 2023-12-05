using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Horus.Core.Services;
using Horus.Enums;
using Horus.Models;
using Horus.Services;
using Horus.Shared.Enums;
using Horus.Shared.Models.Data;
using ReactiveUI;
using Serilog;

namespace Horus.ViewModels;

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

    private MetadataModel CreatedMetadata => InternalMetadatas.First(m => m is { Type: MetadataType.Internal, Key: "created" });
    public string CreatedMetadataDisplay => CreatedMetadata.DisplayValue;
    private MetadataModel ModifiedMetadata => InternalMetadatas.First(m => m is { Type: MetadataType.Internal, Key: "modified" });
    public string ModifiedMetadataDisplay => ModifiedMetadata.DisplayValue;

    private string _username = string.Empty;

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private string _url = string.Empty;

    public string Url
    {
        get => _url;
        set => this.RaiseAndSetIfChanged(ref _url, value);
    }

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
        set => this.RaiseAndSetIfChanged(ref _newEntryName, value);
    }

    #endregion

    #region Constructors

    public EntryFormViewModel()
    {
        RetrieveSettingsValues();
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
            Log.Error("Failed to rename entry '{Name}' to '{NewName}': {Message}", EntryName, NewEntryName, result.Error!.Message);
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

        var (settings, settingsError) = AppService.Instance.GetSettings();

        if (error is null)
        {
            if (settingsError is not null)
            {
                Log.Warning("Failed to retrieve settings: {Message}", settingsError.Message);
                SnackbarService.Instance.Show("Password copied", SnackbarSeverity.Success);
            }
            else
            {
                SnackbarService.Instance.Show($"Password copied for {settings!.ClearTimeout}s", SnackbarSeverity.Success);
            }

            return;
        }

        Log.Warning("Failed to copy password '{Name}': {Message}", EntryName, error.Message);
        SnackbarService.Instance.Show("Failed to copy password", SnackbarSeverity.Warning);
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
            Log.Warning("Failed to generate new password for '{Name}': {Messages}", EntryName, error.Message);
            SnackbarService.Instance.Show("Failed to generate new password", SnackbarSeverity.Warning);
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
            Log.Error("Failed to save the password '{Name}'}': {Message}", EntryName, result.Error!.Message);
            SnackbarService.Instance.Show("Failed to save the password", SnackbarSeverity.Error, 5000);
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
                    .Concat(new[]
                    {
                        new MetadataModel("username", Username, MetadataType.Username),
                        new MetadataModel("url", Url, MetadataType.Url),
                    })
                    .Concat(Metadatas.ToList())
                    .Select(e => new Metadata(e.Key, e.Value, e.Type))
                    .Where(m => !string.IsNullOrWhiteSpace(m.Key) || !string.IsNullOrWhiteSpace(m.Value))
                    .ToList()
            )
        );
        IsLoading = false;

        if (result.HasError)
        {
            Log.Error("Failed to save the metadata for '{Name}': {Message}", EntryName, result.Error!.Message);
            SnackbarService.Instance.Show("Failed to save the metadata", SnackbarSeverity.Error, 5000);
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

        if (error is not null)
        {
            Log.Error("Failed to read metadata for '{Name}': {Message}", EntryName, error.Message);
            SnackbarService.Instance.Show("Failed to read metadata", SnackbarSeverity.Error, 5000);
            return;
        }

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

                switch (metadata.Type)
                {
                    case MetadataType.Internal:
                        InternalMetadatas.Add(item);
                        break;

                    case MetadataType.Normal:
                        Metadatas.Add(item);
                        break;

                    case MetadataType.Username:
                        Username = item.Value;
                        break;

                    case MetadataType.Url:
                        Url = item.Value;
                        break;

                    default:
                        Log.Warning("Unknown metadata type: {Type}", metadata.Type);
                        break;
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
        Username = string.Empty;
        Url = string.Empty;
        AreMetadatasRevealed = false;
        this.RaisePropertyChanged(nameof(HasNormalMetadatas));
    }

    #endregion

    #region Private methods

    private void RetrieveSettingsValues()
    {
        var (settings, error) = AppService.Instance.GetSettings();
        if (error is not null)
        {
            Log.Warning("Failed to retrieve settings: {Message}", error.Message);
            SnackbarService.Instance.Show("Failed to retrieve settings", SnackbarSeverity.Warning);
            return;
        }

        CustomPasswordAlphabet = settings!.DefaultCustomAlphabet;
        PasswordLength = settings.DefaultLength;
    }

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