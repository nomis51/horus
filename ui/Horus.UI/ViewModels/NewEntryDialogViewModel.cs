using ReactiveUI;

namespace Horus.UI.ViewModels;

public class NewEntryDialogViewModel : ViewModelBase
{
    #region Public  methods

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.RaisePropertyChanged(nameof(IsNameValid));
        }
    }

    public bool IsNameValid => !string.IsNullOrWhiteSpace(Name);

    #endregion
}