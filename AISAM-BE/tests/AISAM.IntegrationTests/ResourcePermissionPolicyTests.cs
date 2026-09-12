using AISAM.Data.Enumeration;
using AISAM.Services.Access;

namespace AISAM.IntegrationTests;

public class ResourcePermissionPolicyTests
{
    private static ResourcePermissionFacts Facts(WorkspaceMemberRoleEnum role) => new(
        role, WorkspaceStatusEnum.Active, true, true, true, true, true, true, true);

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.Owner)]
    [InlineData(WorkspaceMemberRoleEnum.Manager)]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer)]
    public void MembershipAndTenantBoundaryCannotBeBypassed(WorkspaceMemberRoleEnum role)
    {
        foreach (var action in Enum.GetValues<ResourcePermission>())
        {
            Assert.False(ResourcePermissionPolicy.Allows(Facts(role) with { ActiveMembership = false }, action));
            Assert.False(ResourcePermissionPolicy.Allows(Facts(role) with { ResourceInWorkspace = false }, action));
            Assert.False(ResourcePermissionPolicy.Allows(Facts(role) with { WorkspaceStatus = WorkspaceStatusEnum.Deleted }, action));
        }
    }

    [Fact]
    public void OwnerBillingSurvivesReadOnlyButContentWritesDoNot()
    {
        foreach (var status in new[] { WorkspaceStatusEnum.Limited, WorkspaceStatusEnum.Archived, WorkspaceStatusEnum.EligibleForDeletion })
        {
            var owner = Facts(WorkspaceMemberRoleEnum.Owner) with { WorkspaceStatus = status, BrandAccessible = false };
            Assert.True(ResourcePermissionPolicy.Allows(owner, ResourcePermission.BillingManage));
            Assert.False(ResourcePermissionPolicy.Allows(owner, ResourcePermission.PostPublish));
            Assert.False(ResourcePermissionPolicy.Allows(owner, ResourcePermission.ContentEdit));
            Assert.False(ResourcePermissionPolicy.Allows(owner with { Role = WorkspaceMemberRoleEnum.Manager }, ResourcePermission.BillingManage));
        }
    }

    [Fact]
    public void ManagerAndDelegatedCreatorCannotEscapeBrandScope()
    {
        foreach (var role in new[] { WorkspaceMemberRoleEnum.Manager, WorkspaceMemberRoleEnum.ContentCreator })
            foreach (var action in Enum.GetValues<ResourcePermission>())
                Assert.False(ResourcePermissionPolicy.Allows(Facts(role) with
                { BrandAccessible = false, CanViewAllCreators = true, CanReview = true, CanPublish = true }, action));
    }

    [Fact]
    public void CreatorReadDelegationDoesNotGrantEditOrPublish()
    {
        var creator = Facts(WorkspaceMemberRoleEnum.ContentCreator) with { OwnContent = false };
        Assert.False(ResourcePermissionPolicy.Allows(creator, ResourcePermission.ContentView));
        creator = creator with { CanViewAllCreators = true, CanPublish = true };
        Assert.True(ResourcePermissionPolicy.Allows(creator, ResourcePermission.ContentView));
        Assert.False(ResourcePermissionPolicy.Allows(creator, ResourcePermission.ContentEdit));
        Assert.False(ResourcePermissionPolicy.Allows(creator, ResourcePermission.PostPublish));
    }

    [Fact]
    public void PublishRequiresChannelGrantAndCreatorActionGrant()
    {
        var creator = Facts(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.False(ResourcePermissionPolicy.Allows(creator, ResourcePermission.PostPublish));
        Assert.True(ResourcePermissionPolicy.Allows(creator with { CanPublish = true }, ResourcePermission.PostPublish));
        var manager = Facts(WorkspaceMemberRoleEnum.Manager);
        Assert.True(ResourcePermissionPolicy.Allows(manager, ResourcePermission.PostPublish));
        Assert.False(ResourcePermissionPolicy.Allows(manager with { ChannelCanPublish = false }, ResourcePermission.PostPublish));
        Assert.False(ResourcePermissionPolicy.Allows(manager with { ChannelAccessible = false }, ResourcePermission.PostPublish));
    }

    [Fact]
    public void ViewerCannotSeeCreatorHistoryEvenWithDelegationFlags()
    {
        var viewer = Facts(WorkspaceMemberRoleEnum.Viewer) with { CanViewAllCreators = true, CanPublish = true, CanReview = true };
        Assert.True(ResourcePermissionPolicy.Allows(viewer, ResourcePermission.BrandView));
        foreach (var action in new[] { ResourcePermission.ContentView, ResourcePermission.PostView, ResourcePermission.PostPublish,
            ResourcePermission.ApprovalReview, ResourcePermission.AnalyticsView, ResourcePermission.AnalyticsMember })
            Assert.False(ResourcePermissionPolicy.Allows(viewer, action));
    }

    [Fact]
    public void CreatorPerformanceIsPersonalOnly()
    {
        var creator = Facts(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.False(ResourcePermissionPolicy.Allows(creator, ResourcePermission.AnalyticsMember));
        Assert.True(ResourcePermissionPolicy.Allows(creator with { OwnPerformance = true }, ResourcePermission.AnalyticsMember));
    }
}
