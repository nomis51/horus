using System.Threading.Tasks;
using Horus.Enums;
using ReactiveUI;

namespace Horus.ViewModels;

public class SnackbarManagerViewModel : ViewModelBase
{
    #region Props

    private bool _isVisible;

    public bool IsVisible
    {
        get => _isVisible;
        set => this.RaiseAndSetIfChanged(ref _isVisible, value);
    }

    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    private SnackbarSeverity _severity = SnackbarSeverity.Accent;

    private SnackbarSeverity Severity
    {
        get => _severity;
        set
        {
            this.RaiseAndSetIfChanged(ref _severity, value);
            this.RaisePropertyChanged(nameof(IsSeverityError));
            this.RaisePropertyChanged(nameof(IsSeverityAccent));
            this.RaisePropertyChanged(nameof(IsSeverityWarning));
            this.RaisePropertyChanged(nameof(IsSeveritySuccess));
        }
    }

    public bool IsSeverityAccent => Severity == SnackbarSeverity.Accent;
    public bool IsSeverityError => Severity == SnackbarSeverity.Error;
    public bool IsSeveritySuccess => Severity == SnackbarSeverity.Success;
    public bool IsSeverityWarning => Severity == SnackbarSeverity.Warning;

    #endregion

    #region Public methods

    public void ShowSnackbar(string message, SnackbarSeverity severity, int duration)
    {
        Text = message;
        Severity = severity;
        IsVisible = true;

        DeferClose(duration);
    }

    public void CloseSnackbar()
    {
        IsVisible = false;
    }

    #endregion

    #region Private methods

    private void DeferClose(int duration)
    {
        if (duration <= 0)
        {
            IsVisible = false;
            return;
        }

        Task.Run(async () =>
        {
            await Task.Delay(duration);
            InvokeUi(() => { IsVisible = false; });
        });
    }

    #endregion
}