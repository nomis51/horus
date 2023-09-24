using Microsoft.AspNetCore.Components;
using WinPass.UI.Components.Abstractions;

namespace WinPass.UI.Components;

public class PasswordFieldBase : Component
{
    #region Constants

    protected const string PasswordPlaceholder = "12345678901234567890";

    #endregion

    #region Parameters

    [Parameter]
    public bool IsReadOnly { get; set; } = true;
    
    [Parameter]
    public EventCallback<bool> IsValid { get; set; }
    
    #endregion

    #region Members

    protected string NewPassword { get; set; } = string.Empty;
    protected string NewPasswordConfirm { get; set; } = string.Empty;

    #endregion

    #region Protected methods

    protected bool ValidatePassword(string value)
    {
        var result = NewPassword == value;
        IsValid.InvokeAsync(result);
        return result;
    }

    #endregion
}