namespace WinPass.Tests;

public class TestHelper
{
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