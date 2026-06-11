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

    private static CreditService CreateService(AisamContext context)
    {
        return new CreditService(
            new CreditWalletRepository(context),
            new CreditUsageRecordRepository(context),
            new WorkspaceRepository(context));
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
