using Avalonia;
using Avalonia.Interactivity;
using Horus.UI.ViewModels;

namespace Horus.UI.Windows;

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

    private void TitleBar_OnOnWindowDragged(int x, int y)
    {
        Position = new PixelPoint(x, y);
    }

    #endregion
}