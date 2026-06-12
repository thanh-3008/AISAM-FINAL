using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class WorkspaceLifecycleProcessingServiceTests
{
    [Fact]
    public async Task RunBatchAsync_SynchronizesExpiredWorkspacesInBackground()
    {
        await using var context = CreateContext();
        context.Workspaces.AddRange(
            CreateWorkspace("Limited", WorkspaceStatusEnum.Active, DateTime.UtcNow.Date.AddDays(-30)),
            CreateWorkspace("Archived", WorkspaceStatusEnum.Active, DateTime.UtcNow.Date.AddDays(-90)),
            CreateWorkspace("Eligible", WorkspaceStatusEnum.Archived, DateTime.UtcNow.Date.AddDays(-181), DateTime.UtcNow.Date.AddDays(-91)),
            CreateWorkspace("Active", WorkspaceStatusEnum.Active, DateTime.UtcNow.Date.AddDays(10)));
        await context.SaveChangesAsync();

        var service = new WorkspaceLifecycleProcessingService(
            new WorkspaceRepository(context),
            new WorkspaceLifecycleService());

        var updatedCount = await service.RunBatchAsync(20);

        Assert.Equal(3, updatedCount);

        var workspaces = await context.Workspaces.OrderBy(item => item.Name).ToListAsync();
        Assert.Equal(WorkspaceStatusEnum.Active, workspaces.Single(item => item.Name == "Active").Status);
        Assert.Equal(WorkspaceStatusEnum.Limited, workspaces.Single(item => item.Name == "Limited").Status);
        Assert.Equal(WorkspaceStatusEnum.Archived, workspaces.Single(item => item.Name == "Archived").Status);
        Assert.Equal(WorkspaceStatusEnum.EligibleForDeletion, workspaces.Single(item => item.Name == "Eligible").Status);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static Workspace CreateWorkspace(
        string name,
        WorkspaceStatusEnum status,
        DateTime? subscriptionExpiredAt,
        DateTime? archivedAt = null)
    {
        return new Workspace
        {
            Name = name,
            WorkspaceType = WorkspaceTypeEnum.Business,
            Status = status,
            MemberLimit = 10,
            SubscriptionExpiredAt = subscriptionExpiredAt,
            ArchivedAt = archivedAt
        };
    }
}
