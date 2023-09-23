using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using WinPass.UI.Extensions;

namespace WinPass.UI.Windows;

public partial class MainWindow
{
    #region Constructors

    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();
    }

    #endregion

    #region Private methods

    private void InitializeServices()
    {
        var services = new ServiceCollection();
        services.AddServices();
        Resources.Add("services", services.BuildServiceProvider());
    }

    #endregion
}