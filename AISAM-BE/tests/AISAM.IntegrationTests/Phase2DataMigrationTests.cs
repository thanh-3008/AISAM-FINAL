using System;
using System.Linq;
using System.Threading.Tasks;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class Phase2DataMigrationTests
{
    [Fact]
    public async Task Phase2Migration_CorrectlyBackfillsContentAndRemapsRoles()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new AisamContext(options);

        // 1. Setup fixture with legacy data
        var workspace = new Workspace { Name = "Test Workspace", Status = WorkspaceStatusEnum.Active };
        var ownerUser = new User { Email = "owner@test.com" };
        var managerUser = new User { Email = "manager@test.com" };
        var creatorUser = new User { Email = "creator@test.com" };
        var viewerUser = new User { Email = "viewer@test.com" };

        var ownerMember = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = ownerUser.Id, Role = (WorkspaceMemberRoleEnum)1, IsActive = true };
        var managerMember = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = managerUser.Id, Role = (WorkspaceMemberRoleEnum)2, IsActive = true };
        var creatorMember = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creatorUser.Id, Role = (WorkspaceMemberRoleEnum)3, IsActive = true };
        var viewerMember = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = viewerUser.Id, Role = (WorkspaceMemberRoleEnum)4, IsActive = true };

        var brandA = new Brand { WorkspaceId = workspace.Id, Name = "Brand A", IsDeleted = false };
        var brandB = new Brand { WorkspaceId = workspace.Id, Name = "Brand B", IsDeleted = false };

        var teamA = new Team { WorkspaceId = workspace.Id, Name = "Team Alpha", Status = TeamStatusEnum.Active };
        var teamBrandA = new TeamBrand { TeamId = teamA.Id, BrandId = brandA.Id, IsActive = true };

        // 2 contents with NULL team_id (D-02 simulation)
        var contentA = new Content
        {
            WorkspaceId = workspace.Id,
            BrandId = brandA.Id,
            TextContent = "Legacy post for Brand A",
            TeamId = null
        };
        var contentB = new Content
        {
            WorkspaceId = workspace.Id,
            BrandId = brandB.Id,
            TextContent = "Legacy post for Brand B (no existing team)",
            TeamId = null
        };

        db.AddRange(workspace, ownerUser, managerUser, creatorUser, viewerUser);
        db.AddRange(ownerMember, managerMember, creatorMember, viewerMember);
        db.AddRange(brandA, brandB, teamA, teamBrandA);
        await db.SaveChangesAsync();

        // Simulate legacy contents with NULL team_id
        db.Contents.AddRange(contentA, contentB);
        // Temporarily clear teamId before testing Phase 2 backfill logic
        contentA.TeamId = null;
        contentB.TeamId = null;
        await db.SaveChangesAsync();

        // Explicitly set team_id = null to simulate legacy data in DB
        contentA.TeamId = null;
        contentB.TeamId = null;

        // 2. Execute Phase 2 Data Migration Logic
        // Step A: Ensure Default Team exists if any content lacks team or any brand has no team
        var existingTeams = await db.Teams.Where(t => t.WorkspaceId == workspace.Id && !t.IsDeleted && t.Status == TeamStatusEnum.Active).ToListAsync();
        Team defaultTeam;
        if (existingTeams.Count == 0)
        {
            defaultTeam = new Team
            {
                WorkspaceId = workspace.Id,
                Name = "Default Team",
                Status = TeamStatusEnum.Active
            };
            db.Teams.Add(defaultTeam);
            await db.SaveChangesAsync();
        }
        else
        {
            defaultTeam = existingTeams.First();
        }

        // Step B: Ensure Brand B has a TeamBrand link
        var brandsWithoutTeam = await db.Brands
            .Where(b => b.WorkspaceId == workspace.Id && !b.IsDeleted && !db.TeamBrands.Any(tb => tb.BrandId == b.Id && tb.IsActive))
            .ToListAsync();
        foreach (var b in brandsWithoutTeam)
        {
            db.TeamBrands.Add(new TeamBrand { TeamId = defaultTeam.Id, BrandId = b.Id, IsActive = true });
        }
        await db.SaveChangesAsync();

        // Step C: Backfill Contents with NULL team_id
        var unassignedContents = await db.Contents.Where(c => c.TeamId == null).ToListAsync();
        foreach (var c in unassignedContents)
        {
            var matchedTeamId = await db.TeamBrands
                .Where(tb => tb.BrandId == c.BrandId && tb.IsActive)
                .Select(tb => tb.TeamId)
                .FirstOrDefaultAsync();

            c.TeamId = matchedTeamId != Guid.Empty ? matchedTeamId : defaultTeam.Id;
        }
        await db.SaveChangesAsync();

        // Step D: Promote legacy managers to TeamMember.Role = Manager (D-01)
        var legacyManagers = await db.WorkspaceMembers
            .Where(m => (int)m.Role == 2 && m.IsActive)
            .ToListAsync();
        foreach (var mgr in legacyManagers)
        {
            var teamsInWs = await db.Teams.Where(t => t.WorkspaceId == mgr.WorkspaceId && !t.IsDeleted && t.Status == TeamStatusEnum.Active).ToListAsync();
            foreach (var t in teamsInWs)
            {
                var tm = await db.TeamMembers.SingleOrDefaultAsync(m => m.TeamId == t.Id && m.UserId == mgr.UserId);
                if (tm == null)
                {
                    db.TeamMembers.Add(new TeamMember { TeamId = t.Id, UserId = mgr.UserId, Role = TeamRoleEnum.Manager, IsActive = true });
                }
                else
                {
                    tm.Role = TeamRoleEnum.Manager;
                    tm.IsActive = true;
                }
            }
        }
        await db.SaveChangesAsync();

        // Step E: Remap WorkspaceMember.Role (1 -> 1, 2/3/4 -> 3)
        var allMembers = await db.WorkspaceMembers.ToListAsync();
        foreach (var m in allMembers)
        {
            if ((int)m.Role == 1)
            {
                m.Role = WorkspaceMemberRoleEnum.Owner;
            }
            else
            {
                m.Role = WorkspaceMemberRoleEnum.Member;
            }
        }
        await db.SaveChangesAsync();

        // 3. Assertions & Reconciliation Verification
        // Verify Content backfill
        var verifiedContentA = await db.Contents.SingleAsync(c => c.Id == contentA.Id);
        var verifiedContentB = await db.Contents.SingleAsync(c => c.Id == contentB.Id);
        Assert.NotNull(verifiedContentA.TeamId);
        Assert.Equal(teamA.Id, verifiedContentA.TeamId);
        Assert.NotNull(verifiedContentB.TeamId);
        Assert.Equal(teamA.Id, verifiedContentB.TeamId);

        // Verify remaining unassigned contents is 0
        Assert.Equal(0, await db.Contents.CountAsync(c => c.TeamId == null));

        // Verify Manager promotion
        var mgrTeamMembership = await db.TeamMembers.SingleAsync(tm => tm.UserId == managerUser.Id && tm.TeamId == teamA.Id);
        Assert.Equal(TeamRoleEnum.Manager, mgrTeamMembership.Role);

        // Verify WorkspaceMember Roles: no one should be WorkspaceManager (2), only Owner (1) or Member (3)
        Assert.Equal(WorkspaceMemberRoleEnum.Owner, (await db.WorkspaceMembers.SingleAsync(m => m.UserId == ownerUser.Id)).Role);
        Assert.Equal(WorkspaceMemberRoleEnum.Member, (await db.WorkspaceMembers.SingleAsync(m => m.UserId == managerUser.Id)).Role);
        Assert.Equal(WorkspaceMemberRoleEnum.Member, (await db.WorkspaceMembers.SingleAsync(m => m.UserId == creatorUser.Id)).Role);
        Assert.Equal(WorkspaceMemberRoleEnum.Member, (await db.WorkspaceMembers.SingleAsync(m => m.UserId == viewerUser.Id)).Role);
        Assert.False(await db.WorkspaceMembers.AnyAsync(m => (int)m.Role == 2));
    }
}
