using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Horus.UI.ViewModels;

namespace Horus.UI.Views;

public partial class DuplicateEntryDialogView : ViewBase<DuplicateEntryDialogViewModel>
{
    #region Events

    public delegate void CloseEvent(bool created);

    public event CloseEvent? Close;

    #endregion

    #region Constructors

    public DuplicateEntryDialogView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    #endregion

    #region Public methods

    public void SetEntryName(string name)
    {
        ViewModel!.Name = name;
    }

    #endregion

    #region Private methods

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() => { TextBoxName.Focus(); });
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke(false);
    }

    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        Dispatch(vm =>
        {
            if (!vm!.DuplicateEntry()) return;

            InvokeUi(() => Close?.Invoke(true));
        });
    }

    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close?.Invoke(false);
    }

    #endregion
}