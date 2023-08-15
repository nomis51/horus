using WinPass.Core.Services;
using WinPass.Shared.Extensions;

namespace WinPass.Tests;

public class TestHelper
{
    public const string TestGpgId = "26ABF5CC47DFBB62E7B307A5CAB4B61B7C3CEC7F";
    public const string TestAppFolder = ".winpass-tests";
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public static void EnsureReady()
    {
        Lock.Wait();
    }

    public static void Done()
    {
        Lock.Release();
    }

    public static void CreateTestFolder()
    {
        if (Directory.Exists(GetStorePath())) Directory.Delete(GetStorePath(), true);
        Directory.CreateDirectory(GetStorePath());
        AppService.Instance.Encrypt(Path.Join(GetStorePath(), ".lock"), ".lock".ToBase64(), TestGpgId);
        File.WriteAllText(Path.Join(GetStorePath(), ".gpg-id"), TestGpgId);
    }

    public static string GetStorePath()
    {
        return Environment.ExpandEnvironmentVariables(
            $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{TestAppFolder}/");
    }

    public static string GetStoreMigrationPath()
    {
        return Environment.ExpandEnvironmentVariables(
            $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{TestAppFolder}-migration/");
    }
}