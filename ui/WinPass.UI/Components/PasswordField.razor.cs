using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WinPass.Core.Services;
using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;
using WinPass.UI.Components.Abstractions;
using WinPass.UI.Helpers.JsInterops;

namespace WinPass.UI.Components;

public class PasswordFieldBase : Component
{
    #region Constants

    protected const string PasswordPlaceholder = "12345678901234567890";

    #endregion

    #region Parameters

    [Parameter]
    public string SelectedEntry { get; set; }

    [Parameter]
    public bool IsReadOnly { get; set; } = true;

    [Parameter]
    public EventCallback<bool> IsValid { get; set; }

    #endregion

    #region Members

    protected string NewPassword { get; set; } = string.Empty;
    protected string NewPasswordConfirm { get; set; } = string.Empty;
    protected bool IsPasswordVisible { get; private set; }

    #endregion

    #region Public methods

    public EmptyResult Save()
    {
        NewPasswordConfirm = string.Empty;
        return AppService.Instance.EditPassword(SelectedEntry, new Password(NewPassword));
    }

    #endregion

    #region Protected methods

    protected void PasswordIconClick()
    {
        if (IsReadOnly)
        {
            CopyPassword();
        }
        else
        {
            IsPasswordVisible = !IsPasswordVisible;
        }
    }

    protected bool ValidatePassword(string value)
    {
        var result = NewPassword == value;
        IsValid.InvokeAsync(result);
        return result;
    }

    #endregion

    #region Private methods

    private void CopyPassword()
    {
        var (_, error) = AppService.Instance.GetPassword(SelectedEntry);
        if (error is not null)
        {
            Snackbar.Add("Unable to retrieve the password", Severity.Error);
            return;
        }

        Snackbar.Add("Password copied", Severity.Success);
    }

    #endregion
}