using WinPass.UI.ViewModels;

namespace WinPass.UI.Views;

public partial class EntryFormView : ViewBase<EntryFormViewModel>
{
    public EntryFormView()
    {
        InitializeComponent();
    }

    public void SetEntryItem(string name)
    {
        ViewModel?.SetEntryItem(name);
    }
}