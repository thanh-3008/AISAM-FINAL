using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Services.Service;

namespace AISAM.IntegrationTests;

public class WorkspaceLifecycleServiceTests
{
    [Theory]
    [InlineData(-1, WorkspaceLifecycleState.Active)]
    [InlineData(0, WorkspaceLifecycleState.Limited)]
    [InlineData(89, WorkspaceLifecycleState.Limited)]
    [InlineData(90, WorkspaceLifecycleState.Archived)]
    [InlineData(180, WorkspaceLifecycleState.Archived)]
    [InlineData(181, WorkspaceLifecycleState.EligibleForAdminDeletion)]
    public void ResolveState_ReturnsExpectedLifecycleState_AroundExpirationBoundaries(
        int daysAfterExpiration,
        WorkspaceLifecycleState expectedState)
    {
        var expiredAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var workspace = new Workspace
        {
            Status = WorkspaceStatusEnum.Active,
            SubscriptionExpiredAt = expiredAt
        };
        var service = new WorkspaceLifecycleService();

        var result = service.ResolveState(workspace, expiredAt.AddDays(daysAfterExpiration));

        Assert.Equal(expectedState, result);
    }

    [Fact]
    public void ResolveState_ReturnsDeleted_WhenWorkspaceAlreadyDeleted()
    {
        var workspace = new Workspace
        {
            Status = WorkspaceStatusEnum.Deleted,
            DeletedAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var service = new WorkspaceLifecycleService();

        var result = service.ResolveState(workspace, new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(WorkspaceLifecycleState.Deleted, result);
    }

    [Fact]
    public void ResolveState_ReturnsArchived_WhenWorkspaceArchivedManually()
    {
        var workspace = new Workspace
        {
            Status = WorkspaceStatusEnum.Archived,
            ArchivedAt = new DateTime(2026, 8, 30, 0, 0, 0, DateTimeKind.Utc),
            SubscriptionExpiredAt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var service = new WorkspaceLifecycleService();

        var result = service.ResolveState(workspace, new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(WorkspaceLifecycleState.Archived, result);
    }

    [Fact]
    public void ResolveState_ReturnsActive_WhenWorkspaceHasNoExpirationTimestamp()
    {
        var workspace = new Workspace
        {
            Status = WorkspaceStatusEnum.Active,
            SubscriptionExpiredAt = null
        };
        var service = new WorkspaceLifecycleService();

        var result = service.ResolveState(workspace, new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(WorkspaceLifecycleState.Active, result);
    }

    [Theory]
    [InlineData(WorkspaceStatusEnum.Limited, WorkspaceLifecycleState.Limited)]
    [InlineData(WorkspaceStatusEnum.EligibleForDeletion, WorkspaceLifecycleState.EligibleForAdminDeletion)]
    public void ResolveState_UsesPersistedLifecycleStatus_WhenWorkspaceHasNoExpirationTimestamp(
        WorkspaceStatusEnum status,
        WorkspaceLifecycleState expected)
    {
        var workspace = new Workspace
        {
            Status = status,
            SubscriptionExpiredAt = null
        };
        var service = new WorkspaceLifecycleService();

        var result = service.ResolveState(workspace, new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc));

        Assert.Equal(expected, result);
    }
}
