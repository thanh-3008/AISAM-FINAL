using AISAM.Common;
using AISAM.Common.Models;

namespace AISAM.Services.IServices;

public interface IAutomationApprovalService
{
    Task<GenericResponse<AutomationPlanDto>> ApproveAsync(Guid workspaceId, Guid planId, Guid approverUserId, Guid? itemId = null, IReadOnlyCollection<Guid>? integrationIds = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<AutomationPlanDto>> RejectAsync(Guid workspaceId, Guid planId, Guid itemId, Guid approverUserId, string? notes = null, CancellationToken cancellationToken = default);
    Task<GenericResponse<IReadOnlyList<AutomationTargetDto>>> GetTargetsAsync(Guid workspaceId, Guid planId, Guid itemId, CancellationToken cancellationToken = default);
}
