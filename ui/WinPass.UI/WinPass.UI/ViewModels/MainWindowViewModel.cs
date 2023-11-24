namespace WinPass.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Props

    public EntryListViewModel EntryListViewModel { get; set; } = new();

    #endregion
}