using AISAM.Common;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IAutomationService
{
    Task<GenericResponse<AutomationPlanDto>> CreateAsync(Guid workspaceId, Guid profileId, CreateAutomationPlanRequest request, string? sourceFileName = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> ImportCsvAsync(Guid workspaceId, Guid profileId, string name, string timezone, string sourceFileName, Stream stream, CancellationToken cancellationToken = default);
    Task<GenericResponse<IReadOnlyList<AutomationPlanDto>>> GetAllAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> GetByIdAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> ConfirmAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> RetryAsync(Guid workspaceId, Guid planId, Guid? itemId = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> CancelAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> ImportGoogleSheetAsync(Guid workspaceId, Guid profileId, ImportGoogleSheetRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> CloneAsync(Guid workspaceId, Guid profileId, Guid planId, CloneAutomationPlanRequest request, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> SetAutoApproveAsync(Guid workspaceId, Guid planId, bool enabled, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPerformanceDto>> GetPerformanceAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> UpdateItemAsync(Guid workspaceId, Guid planId, Guid itemId, UpdateAutomationItemRequest request, CancellationToken cancellationToken = default);
}
