using Avalonia.Interactivity;
using WinPass.UI.ViewModels;

namespace WinPass.UI.Windows;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    #region Constructors

    public MainWindow()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void EntryListView_OnOnEntrySelected(string name)
    {
        EntryFormView.SetEntryItem(name);
    }

    private void ButtonCloseSnackbar_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel?.CloseSnackbar();
    }
    #endregion

}