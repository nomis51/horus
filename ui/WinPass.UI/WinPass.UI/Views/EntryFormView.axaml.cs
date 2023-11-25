using Avalonia.Interactivity;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Views;

public partial class EntryFormView : ViewBase<EntryFormViewModel>
{
    #region Constructors

    public EntryFormView()
    {
        InitializeComponent();
    }

    #endregion

    #region Public methods

    public void SetEntryItem(string name)
    {
        ViewModel?.SetEntryItem(name);
    }

    #endregion

    #region Private methods

    private void ButtonRevealMetadatas_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.RetrieveMetadatas();
    }

    private void ButtonAddMetadata_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.AddMetadata();
    }

    #endregion
}