using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service
{
    public sealed class AdminSettingsService : IAdminSettingsService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISystemSettingRepository _systemSettingRepository;

        public AdminSettingsService(IUserRepository userRepository, ISystemSettingRepository systemSettingRepository)
        {
            _userRepository = userRepository;
            _systemSettingRepository = systemSettingRepository;
        }

        public async Task<GenericResponse<object>> GetAllSettingsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var settings = await _systemSettingRepository.GetAllAsync();
            return GenericResponse<object>.CreateSuccess(settings);
        }

        public async Task<GenericResponse<bool>> UpsertSettingAsync(Guid adminUserId, string key, string value, string? description, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var setting = new SystemSetting { Key = key, Value = value, Description = description, UpdatedBy = adminUserId };
            await _systemSettingRepository.UpsertAsync(setting);
            return GenericResponse<bool>.CreateSuccess(true, "Setting saved.");
        }

        public async Task<GenericResponse<bool>> UpsertSettingsBatchAsync(Guid adminUserId, Dictionary<string, string> settings, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            foreach (var kvp in settings)
            {
                var setting = new SystemSetting { Key = kvp.Key, Value = kvp.Value, UpdatedBy = adminUserId };
                await _systemSettingRepository.UpsertAsync(setting);
            }
            return GenericResponse<bool>.CreateSuccess(true, "Settings saved.");
        }
    }
}
