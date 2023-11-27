using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Settings;

public class SettingsSetEnvironmentVariableFailedError : Error
{
    public SettingsSetEnvironmentVariableFailedError(string message) : base(message)
    {
    }
}