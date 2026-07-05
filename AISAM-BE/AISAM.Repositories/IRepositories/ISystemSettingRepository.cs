using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories
{
    public interface ISystemSettingRepository
    {
        Task<SystemSetting?> GetByKeyAsync(string key);
        Task<List<SystemSetting>> GetAllAsync();
        Task<SystemSetting> UpsertAsync(SystemSetting setting);
    }
}
