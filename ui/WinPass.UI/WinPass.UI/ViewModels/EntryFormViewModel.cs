using System;
using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using ReactiveUI;
using WinPass.Core.Services;
using WinPass.Shared.Enums;
using WinPass.UI.Models;

namespace WinPass.UI.ViewModels;

public class EntryFormViewModel : ViewModelBase
{
    #region Props

    private string _entryName = string.Empty;

    public string EntryName
    {
        get => _entryName;
        set => this.RaiseAndSetIfChanged(ref _entryName, value);
    }

    public ObservableCollection<MetadataModel> Metadatas { get; } = new();
    public ObservableCollection<MetadataModel> InternalMetadatas { get; } = new();
    private bool _metadatasRevealed;

    public bool MetadatasRevealed
    {
        get => _metadatasRevealed;
        private set
        {
            this.RaiseAndSetIfChanged(ref _metadatasRevealed, value);
            this.RaisePropertyChanged(nameof(HasMetadatas));
        }
    }

    public bool HasMetadatas => Metadatas.Any() || InternalMetadatas.Any();
    public bool HasNormalMetadatas => Metadatas.Any();
    public int[] MetadatasPlaceholders { get; private set; } = { 0, 0, 0, 0, 0 };

    #endregion

    #region Public methods

    public void SetEntryItem(string name)
    {
        EntryName = name;
    }

    public void RetrieveMetadatas()
    {
        var (metadatas, error) = AppService.Instance.GetMetadatas(EntryName);
        if (error is not null) return;

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
            else if (metadata.Type != MetadataType.Normal)
            {
                Metadatas.Add(item);
            }
        }

        MetadatasRevealed = true;
    }

    public void AddMetadata()
    {
        Metadatas.Add(new MetadataModel());
        this.RaisePropertyChanged(nameof(HasNormalMetadatas));
    }

    #endregion
}