using AISAM.Data.Enumeration;

namespace AISAM.Services.Access;

// Facts must be resolved from persisted resources and authenticated membership,
// never bound directly from an HTTP request. Resource scope is mandatory even
// when an action has been delegated.
public sealed record ResourcePermissionFacts(
    WorkspaceMemberRoleEnum Role,
    WorkspaceStatusEnum WorkspaceStatus,
    bool ActiveMembership,
    bool ResourceInWorkspace,
    bool BrandAccessible,
    bool OwnContent,
    bool ChannelAccessible,
    bool ChannelCanPublish,
    bool ChannelCanManage,
    bool CanViewAllCreators = false,
    bool CanReview = false,
    bool CanPublish = false,
    bool OwnPerformance = false);

public static class ResourcePermissionPolicy
{
    public static bool Allows(ResourcePermissionFacts facts, ResourcePermission permission)
    {
        if (!Enum.IsDefined(permission) || !Enum.IsDefined(facts.Role) ||
            !Enum.IsDefined(facts.WorkspaceStatus) || !facts.ActiveMembership ||
            !facts.ResourceInWorkspace || facts.WorkspaceStatus == WorkspaceStatusEnum.Deleted)
            return false;

        var owner = facts.Role == WorkspaceMemberRoleEnum.Owner;
        var manager = facts.Role == WorkspaceMemberRoleEnum.Manager;
        var creator = facts.Role == WorkspaceMemberRoleEnum.ContentCreator;

        // Renewal remains possible while normal workspace writes are blocked.
        if (permission == ResourcePermission.BillingManage) return owner;
        var read = permission is ResourcePermission.BrandView or ResourcePermission.ContentView
            or ResourcePermission.ContentViewAllCreators or ResourcePermission.PostView
            or ResourcePermission.SocialView or ResourcePermission.AnalyticsView
            or ResourcePermission.AnalyticsMember;
        if (!read && WorkspaceLifecyclePolicy.IsReadOnly(facts.WorkspaceStatus)) return false;
        if (owner) return true;
        if (!facts.BrandAccessible) return false;

        var contentVisible = manager || creator && (facts.OwnContent || facts.CanViewAllCreators);
        return permission switch
        {
            ResourcePermission.BrandView => true,
            ResourcePermission.BrandManage or ResourcePermission.TeamManage => manager,
            ResourcePermission.ContentCreate => manager || creator,
            ResourcePermission.ContentView or ResourcePermission.PostView => contentVisible,
            ResourcePermission.ContentViewAllCreators => manager || creator && facts.CanViewAllCreators,
            ResourcePermission.ContentEdit or ResourcePermission.ContentDelete => manager || creator && facts.OwnContent,
            ResourcePermission.ApprovalReview => manager || creator && facts.CanReview,
            ResourcePermission.PostPublish => facts.ChannelAccessible && facts.ChannelCanPublish &&
                (manager || creator && facts.CanPublish && facts.OwnContent),
            ResourcePermission.SocialView => facts.ChannelAccessible,
            ResourcePermission.SocialManage => manager && facts.ChannelAccessible && facts.ChannelCanManage,
            ResourcePermission.AnalyticsView => manager,
            ResourcePermission.AnalyticsMember => manager || creator && facts.OwnPerformance,
            _ => false
        };
    }
}
