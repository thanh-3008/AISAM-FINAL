using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly AisamContext _context;

        public SystemSettingRepository(AisamContext context)
        {
            _context = context;
        }

        public async Task<SystemSetting?> GetByKeyAsync(string key)
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<List<SystemSetting>> GetAllAsync()
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SystemSetting> UpsertAsync(SystemSetting setting)
        {
            var existing = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == setting.Key);

            if (existing != null)
            {
                existing.Value = setting.Value;
                existing.Description = setting.Description;
                existing.UpdatedBy = setting.UpdatedBy;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.SystemSettings.Add(setting);
            }

            await _context.SaveChangesAsync();
            return existing ?? setting;
        }
    }
}
