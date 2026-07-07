using AISAM.Common;

namespace AISAM.Services.IServices
{
    public interface IAdminSettingsService
    {
        Task<GenericResponse<object>> GetAllSettingsAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> UpsertSettingAsync(Guid adminUserId, string key, string value, string? description, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> UpsertSettingsBatchAsync(Guid adminUserId, Dictionary<string, string> settings, CancellationToken cancellationToken = default);
    }
}
