using Microsoft.Extensions.DependencyInjection;

namespace WinPass.UI.Windows;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
        Resources.Add("services", serviceCollection.BuildServiceProvider());
    }
}