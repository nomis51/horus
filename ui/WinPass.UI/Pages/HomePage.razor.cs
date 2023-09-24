using System.Management.Automation.Language;
using System.Threading.Tasks;
using MudBlazor;
using WinPass.UI.Components;
using Page = WinPass.UI.Components.Abstractions.Page;

namespace WinPass.UI.Pages;

public class HomePageBase : Page
{
    #region Members

    protected string SelectedEntry { get; set; } = string.Empty;

    #endregion

    #region Protected methods

    protected async Task SelectEntry(string entry)
    {
        SelectedEntry = entry;
        await InvokeAsync(StateHasChanged);
    }

  

    #endregion
}