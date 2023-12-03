using System.Runtime.InteropServices;
using Horus.Shared.Models.GitHub;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Horus.Shared.Helpers;

public static class UpdateHelper
{
    #region Constants

    private const string ReleaseUrl = "https://api.github.com/repos/nomis51/horus/releases/latest";

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

    #endregion

    #region Private methods

    private static bool IsNewVersion(Version version, Version releaseVersion)
    {
        return releaseVersion.Major > version.Major ||
               (releaseVersion.Major == version.Major && releaseVersion.Minor > version.Minor) ||
               (releaseVersion.Major == version.Major && releaseVersion.Minor == version.Minor &&
                releaseVersion.Build > version.Build);
    }

    private static async Task<Tuple<Release?, Version?, Version?>> GetVersions()
    {
        var release = await GetRelease();
        if (release is null) return new Tuple<Release?, Version?, Version?>(default, default, default);

        var version = VersionHelper.GetVersion();
        var releaseVersion = VersionHelper.Parse(release.TagName);
        return Tuple.Create(
            release,
            version,
            releaseVersion
        )!;
    }

    private static async Task<Release?> GetRelease()
    {
        HttpClient client = new();
        client.BaseAddress = new Uri(ReleaseUrl);
        client.DefaultRequestHeaders.Add("User-Agent", "request");
        var response = await client.GetAsync(string.Empty);
        if (!response.IsSuccessStatusCode) return default;
        var data = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Release>(data);
    }

    #endregion
}