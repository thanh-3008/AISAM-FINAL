using AISAM.API.Controllers;
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class WorkspaceDashboardTests
{
    [Fact]
    public async Task GetSummaryAsync_AggregatesOnlyActiveWorkspaceData()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var service = new WorkspaceDashboardService(
            new CreditUsageRecordRepository(context),
            new PostRepository(context),
            new WorkspaceMemberRepository(context),
            new FakeQuotaService(),
            new CreditService(
                new CreditWalletRepository(context),
                new CreditUsageRecordRepository(context),
                new WorkspaceMemberRepository(context),
                new WorkspaceRepository(context),
                context));

        var result = await service.GetSummaryAsync(fixture.WorkspaceId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(fixture.WorkspaceId, result.Data.WorkspaceId);
        Assert.Equal(200, result.Data.CreditBalance);
        Assert.Equal(35, result.Data.CreditsUsed);
        Assert.Equal(1, result.Data.PublishedPostCount);
        Assert.Equal(5_000, result.Data.PostQuotaLimit);
        Assert.Equal(4_999, result.Data.PostsRemaining);
        Assert.Equal(3, result.Data.AiUsageCount);
        Assert.Equal(2, result.Data.ActiveMemberCount);
        Assert.Equal(2, result.Data.TopMembers.Count);
        Assert.Equal(fixture.TopUserId, result.Data.TopMembers[0].UserId);
        Assert.Equal(30, result.Data.TopMembers[0].CreditsUsed);
        Assert.Equal(2, result.Data.TopMembers[0].AiUsageCount);
    }

    [Fact]
    public async Task GetSummary_UsesValidatedActiveWorkspaceFromHttpContext()
    {
        var workspaceId = Guid.NewGuid();
        var service = new FakeWorkspaceDashboardService();
        var context = new DefaultHttpContext();
        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;
        var controller = new WorkspaceDashboardController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        await controller.GetSummary();

        Assert.Equal(workspaceId, service.LastWorkspaceId);
    }

    private static DashboardFixture SeedFixture(AisamContext context)
    {
        var workspace = new Workspace { Name = "Dashboard", WorkspaceType = WorkspaceTypeEnum.Business };
        var otherWorkspace = new Workspace { Name = "Other", WorkspaceType = WorkspaceTypeEnum.Business };
        var topUser = AddUser("top@example.com");
        var secondUser = AddUser("second@example.com");
        var inactiveUser = AddUser("inactive@example.com");
        var otherUser = AddUser("other@example.com");

        context.AddRange(workspace, otherWorkspace, topUser, secondUser, inactiveUser, otherUser);
        context.WorkspaceMembers.AddRange(
            AddMember(workspace, topUser),
            AddMember(workspace, secondUser),
            AddMember(workspace, inactiveUser, false),
            AddMember(otherWorkspace, otherUser));
        context.CreditWallets.AddRange(
            new CreditWallet { WorkspaceId = workspace.Id, Balance = 200 },
            new CreditWallet { WorkspaceId = otherWorkspace.Id, Balance = 999 });
        context.CreditUsageRecords.AddRange(
            AddUsage(workspace, topUser, CreditActionEnum.GenerateText, 10, CreditUsageStatusEnum.Success),
            AddUsage(workspace, topUser, CreditActionEnum.GenerateImage, 20, CreditUsageStatusEnum.Success),
            AddUsage(workspace, secondUser, CreditActionEnum.GenerateText, 5, CreditUsageStatusEnum.Success),
            AddUsage(workspace, secondUser, CreditActionEnum.GenerateText, 100, CreditUsageStatusEnum.Failed),
            AddUsage(workspace, secondUser, CreditActionEnum.SubscriptionGrant, 500, CreditUsageStatusEnum.Success),
            AddUsage(otherWorkspace, otherUser, CreditActionEnum.GenerateText, 900, CreditUsageStatusEnum.Success));

        AddPosts(context, workspace, topUser, otherWorkspace, otherUser);
        context.SaveChanges();
        return new DashboardFixture(workspace.Id, topUser.Id);
    }

    private static void AddPosts(
        AisamContext context,
        Workspace workspace,
        User user,
        Workspace otherWorkspace,
        User otherUser)
    {
        var profile = AddProfile(user);
        var otherProfile = AddProfile(otherUser);
        var brand = AddBrand(profile);
        var otherBrand = AddBrand(otherProfile);
        var content = AddContent(profile, brand, workspace);
        var draftContent = AddContent(profile, brand, workspace);
        var otherContent = AddContent(otherProfile, otherBrand, otherWorkspace);
        var integration = AddIntegration(profile, brand);
        var otherIntegration = AddIntegration(otherProfile, otherBrand);

        context.AddRange(profile, otherProfile, brand, otherBrand, content, draftContent, otherContent, integration, otherIntegration);
        context.Posts.AddRange(
            AddPost(content, integration, ContentStatusEnum.Published),
            AddPost(draftContent, integration, ContentStatusEnum.Draft),
            AddPost(otherContent, otherIntegration, ContentStatusEnum.Published));
    }

    private static User AddUser(string email) => new()
    {
        Email = email,
        FullName = email.Split('@')[0],
        PasswordHash = "hash",
        PasswordSalt = "salt"
    };

    private static WorkspaceMember AddMember(Workspace workspace, User user, bool active = true) => new()
    {
        WorkspaceId = workspace.Id,
        Workspace = workspace,
        UserId = user.Id,
        User = user,
        Role = WorkspaceMemberRoleEnum.Viewer,
        IsActive = active
    };

    private static CreditUsageRecord AddUsage(
        Workspace workspace,
        User user,
        CreditActionEnum action,
        long credits,
        CreditUsageStatusEnum status) => new()
    {
        WorkspaceId = workspace.Id,
        Workspace = workspace,
        UserId = user.Id,
        User = user,
        Action = action,
        Credits = credits,
        Status = status
    };

    private static Profile AddProfile(User user) => new()
    {
        UserId = user.Id,
        User = user,
        Name = user.FullName!,
        ProfileType = ProfileTypeEnum.Basic,
        Status = ProfileStatusEnum.Active
    };

    private static Brand AddBrand(Profile profile) => new()
    {
        ProfileId = profile.Id,
        Profile = profile,
        Name = "Brand"
    };

    private static Content AddContent(Profile profile, Brand brand, Workspace workspace) => new()
    {
        ProfileId = profile.Id,
        Profile = profile,
        BrandId = brand.Id,
        Brand = brand,
        WorkspaceId = workspace.Id,
        Workspace = workspace,
        AdType = AdTypeEnum.TextOnly,
        TextContent = "Post"
    };

    private static SocialIntegration AddIntegration(Profile profile, Brand brand) => new()
    {
        ProfileId = profile.Id,
        Profile = profile,
        BrandId = brand.Id,
        Brand = brand,
        Platform = SocialPlatformEnum.Facebook,
        ExternalId = Guid.NewGuid().ToString("N"),
        AccessToken = "token"
    };

    private static Post AddPost(Content content, SocialIntegration integration, ContentStatusEnum status) => new()
    {
        ContentId = content.Id,
        Content = content,
        IntegrationId = integration.Id,
        Integration = integration,
        PublishedAt = DateTime.UtcNow,
        Status = status
    };

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AisamContext(options);
    }

    private sealed record DashboardFixture(Guid WorkspaceId, Guid TopUserId);

    private sealed class FakeWorkspaceDashboardService : IWorkspaceDashboardService
    {
        public Guid LastWorkspaceId { get; private set; }

        public Task<GenericResponse<WorkspaceDashboardSummaryDto>> GetSummaryAsync(
            Guid workspaceId,
            CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            return Task.FromResult(GenericResponse<WorkspaceDashboardSummaryDto>.CreateSuccess(new WorkspaceDashboardSummaryDto()));
        }
    }

    private sealed class FakeQuotaService : IQuotaService
    {
        public Task<GenericResponse<QuotaSummaryDto>> GetWorkspaceSummaryAsync(
            Guid workspaceId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(GenericResponse<QuotaSummaryDto>.CreateSuccess(new QuotaSummaryDto
            {
                PostQuotaLimit = 5_000,
                PostUsage = 1,
                PostRemaining = 4_999
            }));

        public Task<GenericResponse<QuotaSummaryDto>> GetSummaryAsync(Guid profileId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<GenericResponse<bool>> EnsurePromptQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<GenericResponse<bool>> EnsurePostQuotaAsync(Guid profileId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<GenericResponse<bool>> EnsureWorkspacePostQuotaAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
