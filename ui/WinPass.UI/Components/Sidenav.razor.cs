using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class SidenavBase : Component
{
    #region Parameters

    [Parameter]
    public EventCallback<string> OnEntrySelected { get; set; }

    #endregion

    #region Members

    protected EntryList? EntryListRef { get; set; }

    #endregion

    #region Public methods

    public Task RefreshEntries()
    {
        return EntryListRef?.RefreshEntries() ?? Task.CompletedTask;
    }

    #endregion
}