using System.Net;
using System.Security.Claims;
using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class Phase5DependentModulesTests
{
    private static DefaultHttpContext CreateHttpContext(Guid userId, string path, string method = "GET")
    {
        var context = new DefaultHttpContext
        {
            Request =
            {
                Path = path,
                Method = method
            },
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                "TestAuth"))
        };
        return context;
    }

    [Fact]
    public async Task ApprovalWorkflow_TeamManagerWithWorkspaceMemberRoleMember_CanApproveContentViaMiddleware()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var managerUser = new User { Id = Guid.NewGuid(), FullName = "Team Manager" };
        var creatorUser = new User { Id = Guid.NewGuid(), FullName = "Creator" };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand Alpha" };
        var team = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Alpha", Status = TeamStatusEnum.Active };

        var managerMembership = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = managerUser.Id,
            Role = WorkspaceMemberRoleEnum.Member,
            IsActive = true
        };

        var creatorMembership = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = creatorUser.Id,
            Role = WorkspaceMemberRoleEnum.Member,
            IsActive = true
        };

        var teamBrand = new TeamBrand { Id = Guid.NewGuid(), TeamId = team.Id, BrandId = brand.Id, IsActive = true };
        var teamManagerMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = managerUser.Id,
            Role = TeamRoleEnum.Manager,
            IsActive = true
        };

        var content = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = team.Id,
            PrimaryCreatorId = creatorUser.Id,
            Status = ContentStatusEnum.PendingApproval
        };

        db.AddRange(workspace, managerUser, creatorUser, brand, team, managerMembership, creatorMembership, teamBrand, teamManagerMember, content);
        await db.SaveChangesAsync();

        var accessControl = new AccessControlService(db);
        var memberRepo = new DirectWorkspaceMemberRepository(managerMembership);
        var subRepo = new DirectSubscriptionRepository(workspace.Id);

        var nextCalled = false;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext(managerUser.Id, $"/api/content/{content.Id}/approve", "POST");
        context.Request.Headers["X-Workspace-Id"] = workspace.Id.ToString();

        await middleware.InvokeAsync(context, memberRepo, subRepo, accessControl);

        Assert.True(nextCalled);
        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode == 0 ? (int)HttpStatusCode.OK : context.Response.StatusCode);
    }

    [Fact]
    public async Task ApprovalWorkflow_CreatorWithoutDelegatedReview_CannotApproveContentViaMiddleware()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var creatorUser = new User { Id = Guid.NewGuid(), FullName = "Creator" };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand Alpha" };
        var team = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Alpha", Status = TeamStatusEnum.Active };

        var creatorMembership = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = creatorUser.Id,
            Role = WorkspaceMemberRoleEnum.Member,
            IsActive = true
        };

        var teamBrand = new TeamBrand { Id = Guid.NewGuid(), TeamId = team.Id, BrandId = brand.Id, IsActive = true };
        var teamCreatorMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = creatorUser.Id,
            Role = TeamRoleEnum.ContentCreator,
            Permissions = new List<string>(), // no delegated review
            IsActive = true
        };

        var content = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = team.Id,
            PrimaryCreatorId = creatorUser.Id,
            Status = ContentStatusEnum.PendingApproval
        };

        db.AddRange(workspace, creatorUser, brand, team, creatorMembership, teamBrand, teamCreatorMember, content);
        await db.SaveChangesAsync();

        var accessControl = new AccessControlService(db);
        var memberRepo = new DirectWorkspaceMemberRepository(creatorMembership);
        var subRepo = new DirectSubscriptionRepository(workspace.Id);

        var nextCalled = false;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext(creatorUser.Id, $"/api/content/{content.Id}/approve", "POST");
        context.Request.Headers["X-Workspace-Id"] = workspace.Id.ToString();

        await middleware.InvokeAsync(context, memberRepo, subRepo, accessControl);

        Assert.False(nextCalled);
        Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task ApprovalWorkflow_CreatorWithDelegatedReview_CanApproveContentViaMiddleware()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var reviewerUser = new User { Id = Guid.NewGuid(), FullName = "Delegated Reviewer" };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand Alpha" };
        var team = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Alpha", Status = TeamStatusEnum.Active };

        var membership = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = reviewerUser.Id,
            Role = WorkspaceMemberRoleEnum.Member,
            IsActive = true
        };

        var teamBrand = new TeamBrand { Id = Guid.NewGuid(), TeamId = team.Id, BrandId = brand.Id, IsActive = true };
        var teamMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = reviewerUser.Id,
            Role = TeamRoleEnum.ContentCreator,
            Permissions = new List<string> { DelegatedPermissionKeys.Review },
            IsActive = true
        };

        var content = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brand.Id,
            TeamId = team.Id,
            PrimaryCreatorId = reviewerUser.Id,
            Status = ContentStatusEnum.PendingApproval
        };

        db.AddRange(workspace, reviewerUser, brand, team, membership, teamBrand, teamMember, content);
        await db.SaveChangesAsync();

        var accessControl = new AccessControlService(db);
        var memberRepo = new DirectWorkspaceMemberRepository(membership);
        var subRepo = new DirectSubscriptionRepository(workspace.Id);

        var nextCalled = false;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext(reviewerUser.Id, $"/api/content/{content.Id}/approve", "POST");
        context.Request.Headers["X-Workspace-Id"] = workspace.Id.ToString();

        await middleware.InvokeAsync(context, memberRepo, subRepo, accessControl);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ApprovalWorkflow_WorkspaceManager_CanApproveContentViaMiddleware()
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var wsManager = new User { Id = Guid.NewGuid(), FullName = "Workspace Manager" };

        var membership = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = wsManager.Id,
            Role = WorkspaceMemberRoleEnum.WorkspaceManager,
            IsActive = true
        };

        var memberRepo = new DirectWorkspaceMemberRepository(membership);
        var subRepo = new DirectSubscriptionRepository(workspace.Id);

        var nextCalled = false;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext(wsManager.Id, $"/api/content/{Guid.NewGuid()}/approve", "POST");
        context.Request.Headers["X-Workspace-Id"] = workspace.Id.ToString();

        await middleware.InvokeAsync(context, memberRepo, subRepo);

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.Owner, true)]
    [InlineData(WorkspaceMemberRoleEnum.WorkspaceManager, true)]
    [InlineData(WorkspaceMemberRoleEnum.Member, false)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer, false)]
    public async Task Billing_GetPaymentRoutes_EnforcesOwnerAndWorkspaceManagerOnly(WorkspaceMemberRoleEnum role, bool shouldAllow)
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var user = new User { Id = Guid.NewGuid(), FullName = "User" };

        var membership = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = user.Id,
            Role = role,
            IsActive = true
        };

        var memberRepo = new DirectWorkspaceMemberRepository(membership);
        var subRepo = new DirectSubscriptionRepository(workspace.Id);

        var nextCalled = false;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext(user.Id, "/api/payment/history", "GET");
        context.Request.Headers["X-Workspace-Id"] = workspace.Id.ToString();

        await middleware.InvokeAsync(context, memberRepo, subRepo);

        Assert.Equal(shouldAllow, nextCalled);
        if (!shouldAllow)
        {
            Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
        }
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.Owner, true)]
    [InlineData(WorkspaceMemberRoleEnum.WorkspaceManager, false)]
    [InlineData(WorkspaceMemberRoleEnum.Member, false)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer, false)]
    public async Task Billing_PostPaymentRoutes_EnforcesOwnerOnly(WorkspaceMemberRoleEnum role, bool shouldAllow)
    {
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var user = new User { Id = Guid.NewGuid(), FullName = "User" };

        var membership = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            UserId = user.Id,
            Role = role,
            IsActive = true
        };

        var memberRepo = new DirectWorkspaceMemberRepository(membership);
        var subRepo = new DirectSubscriptionRepository(workspace.Id);

        var nextCalled = false;
        var middleware = new ActiveWorkspaceMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = CreateHttpContext(user.Id, "/api/payment/create", "POST");
        context.Request.Headers["X-Workspace-Id"] = workspace.Id.ToString();

        await middleware.InvokeAsync(context, memberRepo, subRepo);

        Assert.Equal(shouldAllow, nextCalled);
        if (!shouldAllow)
        {
            Assert.Equal((int)HttpStatusCode.Forbidden, context.Response.StatusCode);
        }
    }

    [Fact]
    public async Task MemberPerformance_TeamManager_ScopesStrictlyToManagedTeamAndEliminatesCrossTeamLeakage()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var start = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(7);

        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var managerUser = new User { Id = Guid.NewGuid(), FullName = "Manager Alpha" };
        var memberAlpha = new User { Id = Guid.NewGuid(), FullName = "Member Alpha" };
        var memberBeta = new User { Id = Guid.NewGuid(), FullName = "Member Beta" };

        var brandShared = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Shared Brand" };
        var teamAlpha = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Alpha", Status = TeamStatusEnum.Active };
        var teamBeta = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team Beta", Status = TeamStatusEnum.Active };

        db.AddRange(workspace, managerUser, memberAlpha, memberBeta, brandShared, teamAlpha, teamBeta,
            // Workspace memberships: all are 2-tier role 'Member'
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = managerUser.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = memberAlpha.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = memberBeta.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            // Both teams assigned to the same brand
            new TeamBrand { TeamId = teamAlpha.Id, BrandId = brandShared.Id, IsActive = true },
            new TeamBrand { TeamId = teamBeta.Id, BrandId = brandShared.Id, IsActive = true },
            // Manager is Manager in Team Alpha ONLY
            new TeamMember { TeamId = teamAlpha.Id, UserId = managerUser.Id, Role = TeamRoleEnum.Manager, IsActive = true },
            // Member Alpha is Creator in Team Alpha
            new TeamMember { TeamId = teamAlpha.Id, UserId = memberAlpha.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            // Member Beta is Creator in Team Beta
            new TeamMember { TeamId = teamBeta.Id, UserId = memberBeta.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true });

        // Content 1 belongs to Team Alpha (created by memberAlpha)
        var contentAlpha = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brandShared.Id,
            TeamId = teamAlpha.Id,
            PrimaryCreatorId = memberAlpha.Id,
            CreatedAt = start.AddDays(1)
        };

        // Content 2 belongs to Team Beta (created by memberBeta, same brand!)
        var contentBeta = new Content
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            BrandId = brandShared.Id,
            TeamId = teamBeta.Id,
            PrimaryCreatorId = memberBeta.Id,
            CreatedAt = start.AddDays(1)
        };

        db.AddRange(contentAlpha, contentBeta);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var accessControl = new AccessControlService(db);
        var service = new MemberPerformanceService(db, accessControl);

        // Team Manager calls performance without passing teamId
        var result = await service.GetAsync(managerUser.Id, workspace.Id, start, end);

        // 1. Result teams must only list Team Alpha
        Assert.Single(result.Teams);
        Assert.Equal(teamAlpha.Id, result.Teams[0].Id);

        // 2. Permitted members must only include Team Alpha members (managerUser + memberAlpha), NOT memberBeta
        Assert.DoesNotContain(result.Members, m => m.Id == memberBeta.Id);
        Assert.Contains(result.Members, m => m.Id == memberAlpha.Id);

        // 3. Items must only contain memberAlpha's performance, NOT memberBeta
        Assert.DoesNotContain(result.Items, r => r.MemberId == memberBeta.Id);
        var alphaRow = result.Items.Single(r => r.MemberId == memberAlpha.Id);
        Assert.Equal(1, alphaRow.ContentsCreated);
    }

    [Fact]
    public async Task MemberPerformance_Owner_HasUniversalWorkspaceVisibility()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var start = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(7);

        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var ownerUser = new User { Id = Guid.NewGuid(), FullName = "Owner User" };
        var creator = new User { Id = Guid.NewGuid(), FullName = "Creator" };
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Brand" };
        var team = new Team { Id = Guid.NewGuid(), WorkspaceId = workspace.Id, Name = "Team", Status = TeamStatusEnum.Active };

        db.AddRange(workspace, ownerUser, creator, brand, team,
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = ownerUser.Id, Role = WorkspaceMemberRoleEnum.Owner, IsActive = true },
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = creator.Id, Role = WorkspaceMemberRoleEnum.Member, IsActive = true },
            new TeamBrand { TeamId = team.Id, BrandId = brand.Id, IsActive = true },
            new TeamMember { TeamId = team.Id, UserId = creator.Id, Role = TeamRoleEnum.ContentCreator, IsActive = true },
            new Content { WorkspaceId = workspace.Id, BrandId = brand.Id, TeamId = team.Id, PrimaryCreatorId = creator.Id, CreatedAt = start.AddDays(1) },
            new Content { WorkspaceId = workspace.Id, BrandId = brand.Id, PrimaryCreatorId = null, CreatedAt = start.AddDays(1) }); // unattributed

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = new MemberPerformanceService(db, new AccessControlService(db));
        var result = await service.GetAsync(ownerUser.Id, workspace.Id, start, end);

        Assert.Equal(1, result.UnattributedContents);
        var creatorRow = Assert.Single(result.Items, r => r.MemberId == creator.Id);
        Assert.Equal(1, creatorRow.ContentsCreated);
    }

    [Fact]
    public async Task MemberPerformance_PureViewer_ReturnsForbidden403()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var start = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(7);

        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace", Status = WorkspaceStatusEnum.Active };
        var viewer = new User { Id = Guid.NewGuid(), FullName = "Viewer" };

        db.AddRange(workspace, viewer,
            new WorkspaceMember { WorkspaceId = workspace.Id, UserId = viewer.Id, Role = WorkspaceMemberRoleEnum.Viewer, IsActive = true });
        await db.SaveChangesAsync();

        var service = new MemberPerformanceService(db, new AccessControlService(db));
        var ex = await Assert.ThrowsAsync<PerformanceAccessException>(() => service.GetAsync(viewer.Id, workspace.Id, start, end));
        Assert.Equal(403, ex.StatusCode);
    }

    private sealed class DirectWorkspaceMemberRepository : IWorkspaceMemberRepository
    {
        private readonly WorkspaceMember _member;
        public DirectWorkspaceMemberRepository(WorkspaceMember member) => _member = member;

        public Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_member.WorkspaceId == workspaceId && _member.UserId == userId ? _member : null);

        public Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember> AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember> TransferOwnershipAsync(Guid workspaceId, Guid currentOwnerUserId, Guid targetMemberId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class DirectSubscriptionRepository : ISubscriptionRepository
    {
        private readonly Guid _workspaceId;
        public DirectSubscriptionRepository(Guid workspaceId) => _workspaceId = workspaceId;

        public Task<Subscription?> GetCurrentActiveByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult<Subscription?>(workspaceId == _workspaceId
                ? new Subscription { WorkspaceId = workspaceId, Plan = SubscriptionPlanEnum.Plus, IsActive = true }
                : null);

        public Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Subscription>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountSuccessfulPromptUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountSuccessfulPostUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
