using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class CreditServiceTests
{
    [Fact]
    public async Task RecordUsageAsync_StoresCreditUsageMetadataWithoutPromptContent()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var workspace = AddWorkspace(context);
        var service = CreateService(context);

        var result = await service.RecordUsageAsync(
            workspace.Id,
            user.Id,
            CreditActionEnum.GenerateText,
            1,
            CreditUsageStatusEnum.Success);

        Assert.True(result.Success);
        var record = await context.CreditUsageRecords.SingleAsync();
        Assert.Equal(workspace.Id, record.WorkspaceId);
        Assert.Equal(user.Id, record.UserId);
        Assert.Equal(CreditActionEnum.GenerateText, record.Action);
        Assert.Equal(1, record.Credits);
        Assert.Equal(CreditUsageStatusEnum.Success, record.Status);
    }

    [Fact]
    public async Task GrantSubscriptionCreditsAsync_RejectsPersonalWalletThatWouldExceedMaximumBalance()
    {
        await using var context = CreateContext();
        var workspace = AddWorkspace(context, WorkspaceTypeEnum.Personal);
        context.CreditWallets.Add(new CreditWallet
        {
            WorkspaceId = workspace.Id,
            Balance = 14_500
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GrantSubscriptionCreditsAsync(
            workspace.Id,
            Guid.NewGuid(),
            workspace.WorkspaceType,
            SubscriptionPlanEnum.Premium);

        Assert.False(result.Success);
        Assert.Equal("CREDIT_BALANCE_LIMIT_EXCEEDED", result.Error?.ErrorCode);
        Assert.Equal(14_500, (await context.CreditWallets.SingleAsync()).Balance);
    }

    [Fact]
    public async Task EnsureCurrentFreeCreditsAsync_ResetsPersonalFreeWalletAtNewSevenDayCycle()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var workspace = AddWorkspace(context, WorkspaceTypeEnum.Personal);
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Owner
        });
        context.Subscriptions.Add(new Subscription
        {
            WorkspaceId = workspace.Id,
            Plan = SubscriptionPlanEnum.Free,
            QuotaPostsPerMonth = 20,
            StartDate = DateTime.UtcNow.Date.AddDays(-8),
            IsActive = true
        });
        context.CreditWallets.Add(new CreditWallet { WorkspaceId = workspace.Id, Balance = 7 });
        context.CreditUsageRecords.Add(new CreditUsageRecord
        {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Action = CreditActionEnum.SubscriptionGrant,
            Credits = 50,
            Status = CreditUsageStatusEnum.Success,
            CreatedAt = DateTime.UtcNow.Date.AddDays(-8)
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var wallet = await service.EnsureCurrentFreeCreditsAsync(workspace.Id);

        Assert.Equal(50, wallet.Balance);
        Assert.Equal(2, await context.CreditUsageRecords.CountAsync());
    }

    [Fact]
    public async Task EnsureCurrentFreeCreditsAsync_DoesNotResetExpiredFreeSubscription()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var workspace = AddWorkspace(context, WorkspaceTypeEnum.Personal);
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Owner
        });
        context.Subscriptions.Add(new Subscription
        {
            WorkspaceId = workspace.Id,
            Plan = SubscriptionPlanEnum.Free,
            StartDate = DateTime.UtcNow.Date.AddDays(-15),
            EndDate = DateTime.UtcNow.Date.AddDays(-1),
            IsActive = true
        });
        context.CreditWallets.Add(new CreditWallet { WorkspaceId = workspace.Id, Balance = 7 });
        await context.SaveChangesAsync();

        var wallet = await CreateService(context).EnsureCurrentFreeCreditsAsync(workspace.Id);

        Assert.Equal(7, wallet.Balance);
        Assert.Empty(context.CreditUsageRecords);
    }

    [Fact]
    public async Task ConsumeCreditsAsync_UsesSharedPoolForSharedPoolMember()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var workspace = AddWorkspace(context);
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Viewer,
            QuotaMode = MemberQuotaModeEnum.SharedPool
        });
        context.CreditWallets.Add(new CreditWallet
        {
            WorkspaceId = workspace.Id,
            Balance = 100
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.ConsumeCreditsAsync(
            workspace.Id,
            user.Id,
            CreditActionEnum.GenerateText,
            25);

        Assert.True(result.Success);
        Assert.Equal(75, (await context.CreditWallets.SingleAsync()).Balance);
        Assert.Equal(0, (await context.WorkspaceMembers.SingleAsync()).CreditUsed);
    }

    [Fact]
    public async Task ConsumeCreditsAsync_RejectsLifetimeAssignedMemberWhenLimitExceededEvenIfWorkspaceHasBalance()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var workspace = AddWorkspace(context);
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Viewer,
            QuotaMode = MemberQuotaModeEnum.LifetimeAssignedLimit,
            CreditLimit = 50,
            CreditUsed = 40
        });
        context.CreditWallets.Add(new CreditWallet
        {
            WorkspaceId = workspace.Id,
            Balance = 500
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.ConsumeCreditsAsync(
            workspace.Id,
            user.Id,
            CreditActionEnum.GenerateText,
            20);

        Assert.False(result.Success);
        Assert.Equal("MEMBER_CREDIT_LIMIT_EXCEEDED", result.Error?.ErrorCode);
        Assert.Equal(500, (await context.CreditWallets.SingleAsync()).Balance);
        Assert.Equal(40, (await context.WorkspaceMembers.SingleAsync()).CreditUsed);
    }

    [Fact]
    public async Task ConsumeCreditsAsync_ResetsMonthlyAssignedUsageOnFirstDayOfMonth()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var workspace = AddWorkspace(context);
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceMemberRoleEnum.Viewer,
            QuotaMode = MemberQuotaModeEnum.MonthlyAssignedLimit,
            CreditLimit = 100,
            CreditUsed = 90,
            CreditPeriodStart = new DateTime(2026, 5, 1)
        });
        context.CreditWallets.Add(new CreditWallet
        {
            WorkspaceId = workspace.Id,
            Balance = 500
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.ConsumeCreditsAsync(
            workspace.Id,
            user.Id,
            CreditActionEnum.GenerateText,
            20,
            now: new DateTime(2026, 6, 1, 7, 0, 0, DateTimeKind.Utc));

        Assert.True(result.Success);
        var member = await context.WorkspaceMembers.SingleAsync();
        Assert.Equal(20, member.CreditUsed);
        Assert.Equal(new DateTime(2026, 6, 1), member.CreditPeriodStart);
        Assert.Equal(480, (await context.CreditWallets.SingleAsync()).Balance);
    }

    private static CreditService CreateService(AisamContext context)
    {
        return new CreditService(
            new CreditWalletRepository(context),
            new CreditUsageRecordRepository(context),
            new WorkspaceMemberRepository(context),
            new WorkspaceRepository(context),
            context);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static User AddUser(AisamContext context)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static Workspace AddWorkspace(AisamContext context, WorkspaceTypeEnum workspaceType = WorkspaceTypeEnum.Business)
    {
        var workspace = new Workspace
        {
            Name = "Credits Workspace",
            WorkspaceType = workspaceType
        };
        context.Workspaces.Add(workspace);
        context.SaveChanges();
        return workspace;
    }
}




