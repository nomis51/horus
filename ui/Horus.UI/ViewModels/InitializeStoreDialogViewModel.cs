using ReactiveUI;

namespace Horus.UI.ViewModels;

public class InitializeStoreDialogViewModel : ViewModelBase
{
    #region Props

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private string _gpgId = string.Empty;

    public string GpgId
    {
        get => _gpgId;
        set => this.RaiseAndSetIfChanged(ref _gpgId, value);
    }

    private string _gitUrl = string.Empty;

    public string GitUrl
    {
        get => _gitUrl;
        set => this.RaiseAndSetIfChanged(ref _gitUrl, value);
    }

    #endregion
}