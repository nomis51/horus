using Spectre.Console;
using WinPass.Shared.Abstractions;
using WinPass.Shared.Exceptions.Fs;
using WinPass.Shared.Exceptions.Gpg;
using WinPass.Shared.Models.Fs;

namespace WinPass.Core.Services;

public class FsService : IService
{
    #region Constants

    private const string StoreFolderName = ".password-store";
    private const string GpgIdFileName = ".gpg-id";
    private const string StoreFolderPathTemplate = $"%USERPROFILE%\\{StoreFolderName}\\";
    private readonly string _storeFolderPath;

    #endregion

    #region Constructors

    public FsService()
    {
        _storeFolderPath = Environment.ExpandEnvironmentVariables(StoreFolderPathTemplate);
    }

    #endregion

    #region Public methods

    public string GetPath(string name)
    {
        var path = Path.Join(_storeFolderPath, $"{name}.gpg");
        if (File.Exists(path)) return path;
        
        AnsiConsole.MarkupLine($"[red]password named {name} doesn't exists[/]");
        return string.Empty;

    }

    public IEnumerable<StoreEntry> ListStoreEntries()
    {
        if (!IsStoreInitialized())
        {
            AnsiConsole.MarkupLine("[yellow]Store not initialized[/]");
            return Enumerable.Empty<StoreEntry>();
        }

        List<StoreEntry> entries = new();
        EnumerateGpgFiles(_storeFolderPath, entries);
        return entries;
    }

    public void InitializeStoreFolder(string gpgKey)
    {
        if (IsStoreInitialized())
        {
            AnsiConsole.MarkupLine("[yellow]Store already initialized[/]");
            return;
        }

        CreateStoreFolder();
        if (!AppService.Instance.DoGpgKeyExists(gpgKey)) throw new GpgKeyNotFoundException();

        File.WriteAllText(Path.Join(_storeFolderPath, GpgIdFileName), gpgKey);
    }

    public void Initialize()
    {
    }

    #endregion

    #region Private methods

    private bool IsStoreInitialized()
    {
        if (!Directory.Exists(_storeFolderPath)) return false;

        var gpgIdFilePath = Path.Join(_storeFolderPath, GpgIdFileName);
        return File.Exists(gpgIdFilePath) && File.ReadAllText(gpgIdFilePath).Length != 0;
    }

    private void EnumerateGpgFiles(string path, List<StoreEntry> entries)
    {
        entries.AddRange(from filePath in Directory.EnumerateFiles(path)
            where filePath.EndsWith(".gpg")
            select new StoreEntry(Path.GetFileName(filePath).Split(".gpg").First()));

        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            if (dir.StartsWith(".git")) continue;
            entries.Add(new StoreEntry(Path.GetFileName(dir)!));
            EnumerateGpgFiles(dir, entries.Last().Entries);
        }
    }

    private void CreateStoreFolder()
    {
        if (Directory.Exists(_storeFolderPath))
        {
            var files = Directory.EnumerateFileSystemEntries(_storeFolderPath).ToList();
            if (files.Contains(GpgIdFileName)) throw new FsStoreFolderAlreadyExistsException();

            return;
        }

        Directory.CreateDirectory(_storeFolderPath);
    }

    #endregion
}