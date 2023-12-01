using Horus.Core.Services.Abstractions;
using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Data;
using Horus.Shared.Models.Errors.Settings;
using Newtonsoft.Json;
using Serilog;

namespace Horus.Core.Services;

public class SettingsService : ISettingsService
{
    #region Constants

    private const string SettingsFile = "settings.json";

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
            var appFolder = AppService.Instance.GetAppLocation();
            var settingsFilePath = Path.Join(appFolder, SettingsFile);
            File.WriteAllText(settingsFilePath, data);
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

        try
        {
            var appFolder = AppService.Instance.GetAppLocation();
            var settingsFilePath = Path.Join(appFolder, SettingsFile);
            var data = File.ReadAllText(settingsFilePath);
            return new Result<Settings?, Error?>(JsonConvert.DeserializeObject<Settings>(data));
        }
        catch (Exception e)
        {
            return new Result<Settings?, Error?>(new SettingsGetEnvironmentVariableFailedError(e.Message));
        }
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