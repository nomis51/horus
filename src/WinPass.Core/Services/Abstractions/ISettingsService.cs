using WinPass.Shared.Models.Abstractions;
using WinPass.Shared.Models.Data;

namespace WinPass.Core.Services.Abstractions;

public interface ISettingsService : IService
{
    EmptyResult SaveSettings(Settings settings);
    Result<Settings?, Error?> GetSettings();
    void Initialize();
}