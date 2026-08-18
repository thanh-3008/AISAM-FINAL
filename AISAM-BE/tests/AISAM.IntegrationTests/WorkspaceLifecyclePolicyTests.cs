using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Services;

namespace AISAM.IntegrationTests;

public class WorkspaceLifecyclePolicyTests
{
    private static readonly DateTime UtcNow = new(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(-1, WorkspaceStatusEnum.Limited)]
    [InlineData(-89, WorkspaceStatusEnum.Limited)]
    [InlineData(-90, WorkspaceStatusEnum.Archived)]
    [InlineData(-180, WorkspaceStatusEnum.Archived)]
    [InlineData(-181, WorkspaceStatusEnum.EligibleForDeletion)]
    public void SynchronizeStatus_AppliesBusinessExpirationLifecycle(int expirationOffsetDays, WorkspaceStatusEnum expected)
    {
        var workspace = CreateBusinessWorkspace(UtcNow.AddDays(expirationOffsetDays));

        var changed = WorkspaceLifecyclePolicy.SynchronizeStatus(workspace, UtcNow);

        Assert.True(changed);
        Assert.Equal(expected, workspace.Status);
        Assert.Equal(expected is WorkspaceStatusEnum.Archived or WorkspaceStatusEnum.EligibleForDeletion, workspace.ArchivedAt.HasValue);
    }

    [Fact]
    public void SynchronizeStatus_RestoresActiveStatusWhenSubscriptionWasRenewed()
    {
        var workspace = CreateBusinessWorkspace(UtcNow.AddDays(30));
        workspace.Status = WorkspaceStatusEnum.Archived;
        workspace.ArchivedAt = UtcNow.AddDays(-10);

        WorkspaceLifecyclePolicy.SynchronizeStatus(workspace, UtcNow);

        Assert.Equal(WorkspaceStatusEnum.Active, workspace.Status);
        Assert.Null(workspace.ArchivedAt);
    }

    [Fact]
    public void SynchronizeStatus_DoesNotApplyBusinessLifecycleToPersonalWorkspace()
    {
        var workspace = CreateBusinessWorkspace(UtcNow.AddDays(-181));
        workspace.WorkspaceType = WorkspaceTypeEnum.Personal;

        var changed = WorkspaceLifecyclePolicy.SynchronizeStatus(workspace, UtcNow);

        Assert.False(changed);
        Assert.Equal(WorkspaceStatusEnum.Active, workspace.Status);
    }

    private static Workspace CreateBusinessWorkspace(DateTime expiredAt)
    {
        return new Workspace
        {
            Name = "Business",
            WorkspaceType = WorkspaceTypeEnum.Business,
            Status = WorkspaceStatusEnum.Active,
            SubscriptionExpiredAt = expiredAt
        };
    }
}




