using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace WinPass.Shared.Helpers;

public static class UpdateHelper
{
    #region Constants

    private const string ReleaseUrl = "https://api.github.com/repos/nomis51/winpass/releases/latest";

    #endregion

    #region Public methods

    public static async Task<Tuple<bool, Version?, Version?>> CheckForUpdate()
    {
        var (_, version, releaseVersion) = await GetVersions();
        if (version is null || releaseVersion is null)
            return new Tuple<bool, Version?, Version?>(false, default, default);

        return Tuple.Create(
            IsNewVersion(version, releaseVersion),
            version,
            releaseVersion
        )!;
    }

    public static void EnsureAppLinked()
    {
#if (!DEBUG)
        var assembly = Assembly.GetEntryAssembly()!;
        var filePath = assembly.Location.Replace(".dll", ".exe");

        var dirName = Path.GetDirectoryName(filePath)!;
        AddToPath(dirName);
#endif
    }
    #endregion

    #region Private methods
    

    private static void AddToPath(string path)
    {
        var name = "PATH";
        var scope = EnvironmentVariableTarget.User;
        var oldValue = Environment.GetEnvironmentVariable(name, scope);
        if (string.IsNullOrEmpty(oldValue))
        {
            oldValue = string.Empty;
        }

        if (oldValue.Contains(path)) return;

        oldValue = string.Join(";",
            oldValue.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(v => !v.Contains("WinPass")));

        var newValue = oldValue + $";{path}";
        Environment.SetEnvironmentVariable(name, newValue, scope);
        Log.Information("Application added to PATH");
    }

    private static bool IsNewVersion(Version version, Version releaseVersion)
    {
        return releaseVersion.Major > version.Major ||
               (releaseVersion.Major == version.Major && releaseVersion.Minor > version.Minor) ||
               (releaseVersion.Major == version.Major && releaseVersion.Minor == version.Minor &&
                releaseVersion.Build > version.Build);
    }

    private static async Task<Tuple<JObject?, Version?, Version?>> GetVersions()
    {
        var release = await GetRelease();
        if (release is null) return new Tuple<JObject?, Version?, Version?>(default, default, default);

        var version = VersionHelper.GetVersion();
        var releaseVersion = VersionHelper.Parse(release.Value<string>("tag_name")!);
        return Tuple.Create(
            release,
            version,
            releaseVersion
        )!;
    }

    private static async Task<JObject?> GetRelease()
    {
        HttpClient client = new();
        client.BaseAddress = new Uri(ReleaseUrl);
        client.DefaultRequestHeaders.Add("User-Agent", "request");
        var response = await client.GetAsync(string.Empty);
        if (!response.IsSuccessStatusCode) return default;
        var data = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<JObject>(data);
    }

    #endregion
}