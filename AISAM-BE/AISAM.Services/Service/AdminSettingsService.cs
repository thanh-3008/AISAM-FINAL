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
        private readonly IAuditLogRepository _auditLogRepository;

        private static readonly string[] SecretMarkers = { "key", "secret", "password", "token" };

        public AdminSettingsService(IUserRepository userRepository, ISystemSettingRepository systemSettingRepository, IAuditLogRepository auditLogRepository)
        {
            _userRepository = userRepository;
            _systemSettingRepository = systemSettingRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<GenericResponse<object>> GetAllSettingsAsync(Guid adminUserId, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<object>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var settings = await _systemSettingRepository.GetAllAsync();
            var safeSettings = settings.Select(setting => new
            {
                setting.Id,
                setting.Key,
                Value = IsSecret(setting.Key) ? Mask(setting.Value) : setting.Value,
                IsSecret = IsSecret(setting.Key),
                IsConfigured = !string.IsNullOrWhiteSpace(setting.Value) && setting.Value != "{}",
                setting.Description,
                setting.UpdatedBy,
                setting.UpdatedAt
            }).ToList();
            return GenericResponse<object>.CreateSuccess(safeSettings);
        }

        public async Task<GenericResponse<bool>> UpsertSettingAsync(Guid adminUserId, string key, string value, string? description, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            var setting = new SystemSetting { Key = key, Value = value, Description = description, UpdatedBy = adminUserId };
            var saved = await _systemSettingRepository.UpsertAsync(setting);
            await RecordChangeAsync(adminUserId, saved.Id, cancellationToken);
            return GenericResponse<bool>.CreateSuccess(true, "Setting saved.");
        }

        public async Task<GenericResponse<bool>> UpsertSettingsBatchAsync(Guid adminUserId, Dictionary<string, string> settings, CancellationToken cancellationToken = default)
        {
            var admin = await _userRepository.GetByIdAsync(adminUserId);
            if (admin?.Role != UserRoleEnum.Admin)
                return GenericResponse<bool>.CreateError("Only administrators can access this resource.", HttpStatusCode.Forbidden);

            if (settings.Count == 0 || settings.Count > 100)
                return GenericResponse<bool>.CreateError("Between 1 and 100 settings are required.", HttpStatusCode.BadRequest);
            if (settings.Any(item => string.IsNullOrWhiteSpace(item.Key) || item.Key.Length > 100 || item.Value.Length > 100_000))
                return GenericResponse<bool>.CreateError("One or more settings are invalid.", HttpStatusCode.BadRequest);

            foreach (var kvp in settings)
            {
                if (IsSecret(kvp.Key) && IsMasked(kvp.Value))
                    continue;
                var setting = new SystemSetting { Key = kvp.Key, Value = kvp.Value, UpdatedBy = adminUserId };
                var saved = await _systemSettingRepository.UpsertAsync(setting);
                await RecordChangeAsync(adminUserId, saved.Id, cancellationToken);
            }
            return GenericResponse<bool>.CreateSuccess(true, "Settings saved.");
        }

        private static bool IsSecret(string key) => SecretMarkers.Any(marker => key.Contains(marker, StringComparison.OrdinalIgnoreCase));
        private Task<AuditLog> RecordChangeAsync(Guid actor, Guid settingId, CancellationToken ct) =>
            _auditLogRepository.AddAsync(new AuditLog
            {
                ActorId = actor, ActionType = "UPDATE_SYSTEM_SETTING", TargetTable = "system_settings",
                TargetId = settingId, OldValues = null, NewValues = null,
                // Even a key or description may contain a credential. The stable ID is sufficient.
                Notes = "Configuration updated"
            }, ct);
        private static bool IsMasked(string value) => value.StartsWith("********", StringComparison.Ordinal);
        private static string Mask(string value) => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : $"********{value.Trim('"').TakeLast(Math.Min(4, value.Trim('"').Length)).Aggregate(string.Empty, (text, character) => text + character)}";
    }
}
