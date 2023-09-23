using WinPass.Shared.Models.Abstractions;

namespace WinPass.Shared.Models.Errors.Settings;

public class SettingsSetEnvironmentVariableFailedError : Error
{
    public SettingsSetEnvironmentVariableFailedError(string message) : base(message)
    {
    }
}