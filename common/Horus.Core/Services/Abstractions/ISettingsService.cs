using Horus.Shared.Models.Abstractions;
using Horus.Shared.Models.Data;

namespace Horus.Core.Services.Abstractions;

public interface ISettingsService : IService
{
    EmptyResult SaveSettings(Settings settings);
    Result<Settings?, Error?> GetSettings();
    void Initialize();
}