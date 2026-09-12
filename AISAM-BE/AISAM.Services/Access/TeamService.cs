using System.Text.Json;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Access;

/// <summary>
/// Manages CRUD operations for Team, TeamMember, and related assignments.
/// Enforces workspace scoping, role-based access, and delegation permissions.
/// </summary>
public sealed class TeamService(AisamContext db, IAccessControlService access, ILogger<TeamService>? logger = null)
{
    // ── DTOs ─────────────────────────────────────────────────────────────

    public sealed record CreateTeamRequest(string Name, string? Description, List<TeamMemberInput>? Members);
    public sealed record UpdateTeamRequest(string Name, string? Description);
    public sealed record TeamMemberInput(Guid UserId, string Role);

    public sealed record TeamDto(Guid Id, string Name, string? Description, string Status,
        DateTime CreatedAt, DateTime? UpdatedAt, List<TeamMemberDto> Members,
        List<TeamBrandDto> Brands);

    public sealed record TeamMemberDto(Guid UserId, string Name, string Email, string Role,
        DateTime JoinedAt, bool IsActive);

    public sealed record TeamBrandDto(Guid BrandId, string BrandName, bool IsActive, DateTime AssignedAt);

    public sealed record TeamSummaryDto(Guid Id, string Name, string? Description, string Status,
        int MemberCount, int BrandCount, DateTime CreatedAt, bool HasManager = false);

    // ── Helpers ──────────────────────────────────────────────────────────

    private async Task<WorkspaceMember> RequireMembership(Guid actor, Guid workspace, CancellationToken ct)
    {
        var member = await db.WorkspaceMembers.AsNoTracking()
            .SingleOrDefaultAsync(m => m.UserId == actor && m.WorkspaceId == workspace && m.IsActive, ct);
        if (member is null) throw new UnauthorizedAccessException("Not a workspace member.");
        return member;
    }

    private async Task RequireTeamEntityManage(Guid actor, Guid workspace, Guid teamId, CancellationToken ct)
    {
        var member = await RequireMembership(actor, workspace, ct);
        if (member.Role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.WorkspaceManager)
            return;

        throw new UnauthorizedAccessException("Only Owner or Workspace Manager can manage teams.");
    }

    private async Task RequireTeamMemberManage(Guid actor, Guid workspace, Guid teamId, CancellationToken ct)
    {
        var member = await RequireMembership(actor, workspace, ct);
        if (member.Role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.WorkspaceManager)
            return;

        // Team Manager can manage members in their own team
        var isTeamManager = await db.TeamMembers.AsNoTracking()
            .AnyAsync(m => m.TeamId == teamId && m.UserId == actor && m.Role == TeamRoleEnum.Manager && m.IsActive, ct);
        if (isTeamManager)
            return;

        throw new UnauthorizedAccessException("Only Owner, Workspace Manager, or Team Manager can manage team members.");
    }

    private async Task<bool> HasDelegatedPermission(Guid actor, Guid workspace, string key, CancellationToken ct)
    {
        var teamIds = await (from m in db.TeamMembers.AsNoTracking()
            join t in db.Teams.AsNoTracking() on m.TeamId equals t.Id
            where m.UserId == actor && m.IsActive && t.WorkspaceId == workspace && !t.IsDeleted
                && t.Status == TeamStatusEnum.Active
            select m.TeamId).ToListAsync(ct);
        if (teamIds.Count == 0) return false;
        var permissions = await db.TeamMembers.AsNoTracking()
            .Where(m => m.UserId == actor && m.IsActive && teamIds.Contains(m.TeamId))
            .Select(m => m.Permissions).ToListAsync(ct);
        return permissions.Any(list => list != null && list.Contains(key, StringComparer.Ordinal));
    }

    private void Audit(Guid actor, Guid workspace, string action, string table, Guid targetId,
        object? oldValues = null, object? newValues = null, Guid? teamId = null, Guid? affectedUser = null)
    {
        db.AuditLogs.Add(new AuditLog
        {
            ActorId = actor,
            WorkspaceId = workspace,
            TeamId = teamId,
            AffectedUserId = affectedUser,
            ActionType = action,
            TargetTable = table,
            TargetId = targetId,
            Result = "allowed",
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues)
        });
    }

    // ── List Teams ──────────────────────────────────────────────────────

    public async Task<(List<TeamSummaryDto> Items, int TotalCount)> ListAsync(
        Guid actor, Guid workspace, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        await RequireMembership(actor, workspace, ct);

        var query = db.Teams.AsNoTracking()
            .Where(t => t.WorkspaceId == workspace && !t.IsDeleted);

        var count = await query.CountAsync(ct);
        var teams = await query
            .OrderBy(t => t.Name).ThenBy(t => t.Id)
            .Skip((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 100))
            .Take(Math.Clamp(pageSize, 1, 100))
            .Select(t => new TeamSummaryDto(
                t.Id, t.Name, t.Description, t.Status.ToString(),
                t.TeamMembers.Count(m => m.IsActive),
                t.TeamBrands.Count(b => b.IsActive),
                t.CreatedAt,
                t.TeamMembers.Any(m => m.IsActive && m.Role == TeamRoleEnum.Manager)))
            .ToListAsync(ct);

        return (teams, count);
    }

    // ── Get Team Detail ─────────────────────────────────────────────────

    public async Task<TeamDto?> GetByIdAsync(Guid actor, Guid workspace, Guid teamId, CancellationToken ct = default)
    {
        await RequireMembership(actor, workspace, ct);

        var team = await db.Teams.AsNoTracking()
            .Include(t => t.TeamMembers.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Include(t => t.TeamBrands.Where(b => b.IsActive))
                .ThenInclude(b => b.Brand)
            .SingleOrDefaultAsync(t => t.Id == teamId && t.WorkspaceId == workspace && !t.IsDeleted, ct);

        if (team is null) return null;

        return new TeamDto(
            team.Id, team.Name, team.Description, team.Status.ToString(),
            team.CreatedAt, team.UpdatedAt,
            team.TeamMembers.Select(m => new TeamMemberDto(
                m.UserId, m.User?.FullName ?? m.User?.Email ?? "Unknown",
                m.User?.Email ?? "", m.Role.ToString(), m.JoinedAt, m.IsActive)).ToList(),
            team.TeamBrands.Select(b => new TeamBrandDto(
                b.BrandId, b.Brand?.Name ?? "Unknown", b.IsActive, b.AssignedAt)).ToList());
    }

    // ── Create Team ─────────────────────────────────────────────────────

    public async Task<TeamDto> CreateAsync(Guid actor, Guid workspace, CreateTeamRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Team name is required.");
        if (request.Name.Length > 255)
            throw new ArgumentException("Team name must be at most 255 characters.");

        var member = await RequireMembership(actor, workspace, ct);
        if (member.Role is not (WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.WorkspaceManager))
        {
            throw new UnauthorizedAccessException("Only Owner or Workspace Manager can create teams.");
        }

        // Check duplicate name in workspace
        var exists = await db.Teams.AsNoTracking()
            .AnyAsync(t => t.WorkspaceId == workspace && !t.IsDeleted
                && t.Name.ToLower() == request.Name.Trim().ToLower(), ct);
        if (exists)
            throw new ArgumentException("A team with this name already exists in the workspace.");

        var team = new Team
        {
            WorkspaceId = workspace,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = TeamStatusEnum.Active,
            CreatedAt = DateTime.UtcNow
        };
        db.Teams.Add(team);

        // Add creator as first member
        db.TeamMembers.Add(new TeamMember
        {
            TeamId = team.Id,
            UserId = actor,
            Role = TeamRoleEnum.Manager,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        });

        // Add additional members if specified
        if (request.Members is { Count: > 0 })
        {
            var workspaceMembers = await db.WorkspaceMembers.AsNoTracking()
                .Where(m => m.WorkspaceId == workspace && m.IsActive)
                .Select(m => new { m.Id, m.UserId }).ToListAsync(ct);

            foreach (var mi in request.Members)
            {
                // Validate: match by UserId first, fallback via WorkspaceMember.Id
                var matched = workspaceMembers.FirstOrDefault(m => m.UserId == mi.UserId);
                Guid resolvedUserId;
                if (matched != null)
                {
                    resolvedUserId = matched.UserId;
                }
                else
                {
                    // TEMP compat shim - remove after FE fully migrated, tracked in [ticket]
                    var fallbackMatched = workspaceMembers.FirstOrDefault(m => m.Id == mi.UserId);
                    if (fallbackMatched != null)
                    {
                        logger?.LogWarning(
                            "// TEMP compat shim - remove after FE fully migrated, tracked in [ticket]: Member payload passed WorkspaceMember.Id {MemberId} instead of UserId for workspace {WorkspaceId}. Resolved to UserId {UserId}.",
                            mi.UserId, workspace, fallbackMatched.UserId);
                        resolvedUserId = fallbackMatched.UserId;
                    }
                    else
                    {
                        throw new ArgumentException($"User {mi.UserId} is not a member of this workspace.");
                    }
                }

                if (resolvedUserId == actor) continue; // already added

                db.TeamMembers.Add(new TeamMember
                {
                    TeamId = team.Id,
                    UserId = resolvedUserId,
                    Role = ParseTeamRole(mi.Role),
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
        }

        Audit(actor, workspace, "team.create", "teams", team.Id,
            newValues: new { team.Name, team.Description });

        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(actor, workspace, team.Id, ct))!;
    }

    // ── Update Team ─────────────────────────────────────────────────────

    public async Task<TeamDto> UpdateAsync(Guid actor, Guid workspace, Guid teamId,
        UpdateTeamRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Team name is required.");

        await RequireTeamEntityManage(actor, workspace, teamId, ct);

        var team = await db.Teams
            .SingleOrDefaultAsync(t => t.Id == teamId && t.WorkspaceId == workspace && !t.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Team not found.");

        // Check duplicate name (excluding self)
        var exists = await db.Teams.AsNoTracking()
            .AnyAsync(t => t.WorkspaceId == workspace && !t.IsDeleted && t.Id != teamId
                && t.Name.ToLower() == request.Name.Trim().ToLower(), ct);
        if (exists)
            throw new ArgumentException("A team with this name already exists in the workspace.");

        var oldName = team.Name;
        team.Name = request.Name.Trim();
        team.Description = request.Description?.Trim();
        team.UpdatedAt = DateTime.UtcNow;

        Audit(actor, workspace, "team.update", "teams", team.Id,
            oldValues: new { Name = oldName },
            newValues: new { team.Name, team.Description },
            teamId: teamId);

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(actor, workspace, team.Id, ct))!;
    }

    // ── Delete Team (soft) ──────────────────────────────────────────────

    public async Task DeleteAsync(Guid actor, Guid workspace, Guid teamId, CancellationToken ct = default)
    {
        await RequireTeamEntityManage(actor, workspace, teamId, ct);

        var team = await db.Teams
            .SingleOrDefaultAsync(t => t.Id == teamId && t.WorkspaceId == workspace && !t.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Team not found.");

        team.IsDeleted = true;
        team.UpdatedAt = DateTime.UtcNow;

        // Deactivate all TeamBrand assignments
        var assignments = await db.TeamBrands.Where(tb => tb.TeamId == teamId && tb.IsActive).ToListAsync(ct);
        foreach (var a in assignments)
        {
            a.IsActive = false;
            // Also clear channel access
            var channels = await db.TeamChannelAccesses.Where(c => c.TeamBrandId == a.Id).ToListAsync(ct);
            foreach (var ch in channels)
                ch.CanView = ch.CanPublish = ch.CanManage = false;
        }

        // Deactivate all team members
        var members = await db.TeamMembers.Where(m => m.TeamId == teamId && m.IsActive).ToListAsync(ct);
        foreach (var m in members)
            m.IsActive = false;

        Audit(actor, workspace, "team.delete", "teams", team.Id,
            newValues: new { team.Name, RevokedAssignments = assignments.Count, DeactivatedMembers = members.Count },
            teamId: teamId);

        await db.SaveChangesAsync(ct);
    }

    // ── Add Team Member ─────────────────────────────────────────────────

    public async Task<TeamMemberDto> AddMemberAsync(Guid actor, Guid workspace, Guid teamId,
        Guid userId, string role, CancellationToken ct = default)
    {
        await RequireTeamMemberManage(actor, workspace, teamId, ct);

        // Verify team exists
        var team = await db.Teams.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Id == teamId && t.WorkspaceId == workspace && !t.IsDeleted, ct)
            ?? throw new KeyNotFoundException("Team not found.");

        // Verify target user is a workspace member (match by UserId first, fallback via WorkspaceMember.Id)
        var wsMember = await db.WorkspaceMembers.AsNoTracking()
            .SingleOrDefaultAsync(m => m.UserId == userId && m.WorkspaceId == workspace && m.IsActive, ct);

        Guid resolvedUserId;
        if (wsMember != null)
        {
            resolvedUserId = wsMember.UserId;
        }
        else
        {
            // TEMP compat shim - remove after FE fully migrated, tracked in [ticket]
            var fallback = await db.WorkspaceMembers.AsNoTracking()
                .SingleOrDefaultAsync(m => m.Id == userId && m.WorkspaceId == workspace && m.IsActive, ct);

            if (fallback != null)
            {
                logger?.LogWarning(
                    "// TEMP compat shim - remove after FE fully migrated, tracked in [ticket]: AddMember payload passed WorkspaceMember.Id {MemberId} instead of UserId for workspace {WorkspaceId}. Resolved to UserId {UserId}.",
                    userId, workspace, fallback.UserId);
                wsMember = fallback;
                resolvedUserId = fallback.UserId;
            }
            else
            {
                throw new ArgumentException("User is not an active member of this workspace.");
            }
        }

        // Actor cannot assign a higher workspace role than their own
        var actorMember = await RequireMembership(actor, workspace, ct);
        if (actorMember.Role > wsMember.Role && actorMember.Role != WorkspaceMemberRoleEnum.Owner)
            throw new UnauthorizedAccessException("Cannot add a member with a higher workspace role.");

        // Check if already a member
        var existing = await db.TeamMembers
            .SingleOrDefaultAsync(m => m.TeamId == teamId && m.UserId == resolvedUserId, ct);

        if (existing is not null)
        {
            if (existing.IsActive)
                throw new ArgumentException("User is already an active member of this team.");
            // Reactivate
            existing.IsActive = true;
            existing.Role = ParseTeamRole(role);
            existing.JoinedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new TeamMember
            {
                TeamId = teamId,
                UserId = resolvedUserId,
                Role = ParseTeamRole(role),
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            db.TeamMembers.Add(existing);
        }

        Audit(actor, workspace, "team.member_add", "team_members", teamId,
            newValues: new { UserId = resolvedUserId, Role = existing.Role },
            teamId: teamId, affectedUser: resolvedUserId);

        await db.SaveChangesAsync(ct);

        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == resolvedUserId, ct);
        return new TeamMemberDto(resolvedUserId, user?.FullName ?? user?.Email ?? "Unknown",
            user?.Email ?? "", existing.Role.ToString(), existing.JoinedAt, true);
    }

    // ── Remove Team Member ──────────────────────────────────────────────

    public async Task RemoveMemberAsync(Guid actor, Guid workspace, Guid teamId, Guid userId,
        CancellationToken ct = default)
    {
        await RequireTeamMemberManage(actor, workspace, teamId, ct);

        var member = await db.TeamMembers
            .SingleOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && m.IsActive, ct)
            ?? throw new KeyNotFoundException("Team member not found.");

        member.IsActive = false;

        Audit(actor, workspace, "team.member_remove", "team_members", teamId,
            oldValues: new { UserId = userId, Role = member.Role.ToString() },
            teamId: teamId, affectedUser: userId);

        await db.SaveChangesAsync(ct);
    }

    // ── Update Team Member Role ─────────────────────────────────────────

    public async Task<TeamMemberDto> UpdateMemberRoleAsync(Guid actor, Guid workspace, Guid teamId,
        Guid userId, string newRole, CancellationToken ct = default)
    {
        await RequireTeamMemberManage(actor, workspace, teamId, ct);

        var member = await db.TeamMembers
            .SingleOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId && m.IsActive, ct)
            ?? throw new KeyNotFoundException("Team member not found.");

        var oldRole = member.Role;
        member.Role = ParseTeamRole(newRole);

        Audit(actor, workspace, "team.member_role_update", "team_members", teamId,
            oldValues: new { UserId = userId, Role = oldRole.ToString() },
            newValues: new { Role = member.Role.ToString() },
            teamId: teamId, affectedUser: userId);

        await db.SaveChangesAsync(ct);

        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId, ct);
        return new TeamMemberDto(userId, user?.FullName ?? user?.Email ?? "Unknown",
            user?.Email ?? "", member.Role.ToString(), member.JoinedAt, true);
    }

    private static TeamRoleEnum ParseTeamRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return TeamRoleEnum.ContentCreator;
        if (Enum.TryParse<TeamRoleEnum>(role.Trim(), true, out var parsed)) return parsed;
        return TeamRoleEnum.ContentCreator;
    }
}
