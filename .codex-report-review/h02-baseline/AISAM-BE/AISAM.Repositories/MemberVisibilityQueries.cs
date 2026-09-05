using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    public IQueryable<WorkspaceMemberResponseDto> MemberDirectory(Guid workspaceId)
    {
        if (!AccessScope.Enforced || workspaceId != AccessScope.WorkspaceId)
            throw new UnauthorizedAccessException("A current workspace scope is required.");

        // Basic directory membership is not a grant to read quota or another user's credit usage.
        // Sensitive values are projected conditionally in SQL, before materialization.
        return WorkspaceMembers.AsNoTracking().Where(m => m.WorkspaceId == workspaceId && m.IsActive)
            .OrderBy(m => m.JoinedAt).Select(m => new WorkspaceMemberResponseDto
            {
                Id = m.Id, UserId = m.UserId, Email = m.User.Email, FullName = m.User.FullName,
                Role = m.Role, JoinedAt = m.JoinedAt,
                QuotaMode = AccessScope.IsOwner || AccessScope.Role != WorkspaceMemberRoleEnum.Viewer && m.UserId == AccessScope.UserId ? m.QuotaMode : null,
                CreditLimit = AccessScope.IsOwner || AccessScope.Role != WorkspaceMemberRoleEnum.Viewer && m.UserId == AccessScope.UserId ? m.CreditLimit : null,
                CreditPeriodStart = AccessScope.IsOwner || AccessScope.Role != WorkspaceMemberRoleEnum.Viewer && m.UserId == AccessScope.UserId ? m.CreditPeriodStart : null,
                CreditUsed = AccessScope.IsOwner || AccessScope.Role != WorkspaceMemberRoleEnum.Viewer && m.UserId == AccessScope.UserId ? m.CreditUsed : null
            });
    }
}
