using Horus.Shared.Models.Abstractions;

namespace Horus.Shared.Models.Errors.Settings;

public class SettingsGetEnvironmentVariableFailedError : Error
{
    public SettingsGetEnvironmentVariableFailedError(string message) : base(message)
    {
    }
}