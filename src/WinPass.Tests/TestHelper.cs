using WinPass.Core.Services;
using WinPass.Shared.Extensions;

namespace WinPass.Tests;

public class TestHelper
{
    public const string TestGpgId = "1BF7C718F967FE673D3BD0DBE88096F9B8306F99";
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