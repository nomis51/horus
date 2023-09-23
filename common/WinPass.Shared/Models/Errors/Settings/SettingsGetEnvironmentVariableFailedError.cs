using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Settings;

public class SettingsGetEnvironmentVariableFailedError : Error
{
    public SettingsGetEnvironmentVariableFailedError(string message) : base(message)
    {
    }
}