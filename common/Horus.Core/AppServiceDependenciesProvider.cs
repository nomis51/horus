using Horus.Core.Services.Abstractions;

namespace Horus.Core;

public class AppServiceDependenciesProvider
{
    public IFsService FsService { get; }
    public IGitService GitService { get; }
    public IGpgService GpgService { get; }
    public ISettingsService SettingsService { get; }

    public AppServiceDependenciesProvider(
        IFsService fsService,
        IGitService gitService,
        IGpgService gpgService,
        ISettingsService settingsService)
    {
        FsService = fsService;
        GitService = gitService;
        GpgService = gpgService;
        SettingsService = settingsService;
    }
}