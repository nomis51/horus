using Avalonia.Controls;
using Avalonia.Input;
using Material.Icons.Avalonia;
using WinPass.UI.Extensions;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Views;

public partial class EntryTreeView : ViewBase<EntryTreeViewModel>
{
    #region Constructors

    public EntryTreeView()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void ToggleFolder(string name)
    {
        ViewModel?.ToggleFolder(name);
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        ToggleFolder(sender!.GetTag<string>());
    }

    #endregion
}