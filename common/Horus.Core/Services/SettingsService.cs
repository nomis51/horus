using Horus.Core.Services.Abstractions;
using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Errors.Settings;
using Serilog;

namespace Horus.Core.Services;

public class SettingsService : ISettingsService
{
    #region Constants

    private const string EnvVarName = "HORUS_SETTINGS";

    #endregion

    #region Members

    private Settings? _settings;

    #endregion

    #region Public methods

    public EmptyResult SaveSettings(Settings settings)
    {
        var data = settings.ToString();
        try
        {
            Environment.SetEnvironmentVariable(EnvVarName, data, EnvironmentVariableTarget.User);
        }
        catch (Exception e)
        {
            return new EmptyResult(new SettingsSetEnvironmentVariableFailedError(e.Message));
        }

        return new EmptyResult();
    }

    public Result<Settings?, Error?> GetSettings()
    {
        if (_settings is not null) return new Result<Settings?, Error?>(_settings);

        string? data;
        try
        {
            data = Environment.GetEnvironmentVariable(EnvVarName, EnvironmentVariableTarget.User);
        }
        catch (Exception e)
        {
            return new Result<Settings?, Error?>(new SettingsGetEnvironmentVariableFailedError(e.Message));
        }

        if (string.IsNullOrEmpty(data))
        {
            _settings = new Settings();
            return new Result<Settings?, Error?>(_settings);
        }

        var settings = new Settings();
        var parts = data.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var values = part.Split("=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != 2) continue;

            switch (values[0])
            {
                case nameof(Settings.ClearTimeout):
                    if (!int.TryParse(values[1], out var clearTimeout)) continue;
                    settings.ClearTimeout = clearTimeout;
                    break;

                case nameof(Settings.DefaultLength):
                    if (!int.TryParse(values[1], out var defaultLength)) continue;
                    settings.DefaultLength = defaultLength;
                    break;

                case nameof(Settings.DefaultCustomAlphabet):
                    settings.DefaultCustomAlphabet = values[1];
                    break;

                case nameof(Settings.Language):
                    settings.Language = values[1];
                    break;
            }
        }

        _settings = settings;

        return new Result<Settings?, Error?>(_settings);
    }

    public void Initialize()
    {
        var (settings, error) = GetSettings();
        if (error is not null)
        {
            Log.Warning("Unable to read settings: {Message}", error.Message);
            return;
        }

        _settings = settings;
    }

    #endregion
}