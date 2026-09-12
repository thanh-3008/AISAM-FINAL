using AISAM.Data.Enumeration;

namespace AISAM.Services.Access;

// Facts must be resolved from persisted resources and authenticated membership,
// never bound directly from an HTTP request. Resource scope is mandatory even
// when an action has been delegated.
public sealed record ResourcePermissionFacts
{
    public WorkspaceMemberRoleEnum WorkspaceRole { get; init; }
    public TeamRoleEnum? TeamRole { get; init; }
    public WorkspaceMemberRoleEnum Role { get; init; }
    public WorkspaceStatusEnum WorkspaceStatus { get; init; }
    public bool ActiveMembership { get; init; }
    public bool ResourceInWorkspace { get; init; }
    public bool BrandAccessible { get; init; }
    public bool OwnContent { get; init; }
    public bool SameTeamContent { get; init; }
    public bool ChannelAccessible { get; init; }
    public bool ChannelCanPublish { get; init; }
    public bool ChannelCanManage { get; init; }
    public bool CanViewAllCreators { get; init; }
    public bool CanReview { get; init; }
    public bool CanPublish { get; init; }
    public bool OwnPerformance { get; init; }

    public ResourcePermissionFacts(
        WorkspaceMemberRoleEnum WorkspaceRole,
        TeamRoleEnum? TeamRole,
        WorkspaceStatusEnum WorkspaceStatus,
        bool ActiveMembership,
        bool ResourceInWorkspace,
        bool BrandAccessible,
        bool OwnContent,
        bool SameTeamContent,
        bool ChannelAccessible,
        bool ChannelCanPublish,
        bool ChannelCanManage,
        bool CanViewAllCreators = false,
        bool CanReview = false,
        bool CanPublish = false,
        bool OwnPerformance = false)
    {
        this.WorkspaceRole = WorkspaceRole;
        this.TeamRole = TeamRole;
        this.Role = 0;
        this.WorkspaceStatus = WorkspaceStatus;
        this.ActiveMembership = ActiveMembership;
        this.ResourceInWorkspace = ResourceInWorkspace;
        this.BrandAccessible = BrandAccessible;
        this.OwnContent = OwnContent;
        this.SameTeamContent = SameTeamContent;
        this.ChannelAccessible = ChannelAccessible;
        this.ChannelCanPublish = ChannelCanPublish;
        this.ChannelCanManage = ChannelCanManage;
        this.CanViewAllCreators = CanViewAllCreators;
        this.CanReview = CanReview;
        this.CanPublish = CanPublish;
        this.OwnPerformance = OwnPerformance;
    }

    // Backward-compatible constructor for legacy callers/tests passing WorkspaceMemberRoleEnum Role
    public ResourcePermissionFacts(
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
        bool OwnPerformance = false)
    {
        this.WorkspaceRole = Role == WorkspaceMemberRoleEnum.Owner ? WorkspaceMemberRoleEnum.Owner :
                             Role == WorkspaceMemberRoleEnum.WorkspaceManager ? WorkspaceMemberRoleEnum.WorkspaceManager :
                             WorkspaceMemberRoleEnum.Member;
        this.TeamRole = Role == WorkspaceMemberRoleEnum.Owner || Role == WorkspaceMemberRoleEnum.WorkspaceManager || Role == WorkspaceMemberRoleEnum.Manager ? TeamRoleEnum.Manager :
                        Role == WorkspaceMemberRoleEnum.ContentCreator ? TeamRoleEnum.ContentCreator :
                        // WorkspaceMemberRoleEnum.Viewer is a workspace-scope alias; it does NOT map to
                        // TeamRoleEnum.Viewer so that it never receives team-scoped analytics access.
                        (TeamRoleEnum?)null;
        this.Role = Role;
        this.WorkspaceStatus = WorkspaceStatus;
        this.ActiveMembership = ActiveMembership;
        this.ResourceInWorkspace = ResourceInWorkspace;
        this.BrandAccessible = BrandAccessible;
        this.OwnContent = OwnContent;
        this.SameTeamContent = true;
        this.ChannelAccessible = ChannelAccessible;
        this.ChannelCanPublish = ChannelCanPublish;
        this.ChannelCanManage = ChannelCanManage;
        this.CanViewAllCreators = CanViewAllCreators;
        this.CanReview = CanReview;
        this.CanPublish = CanPublish;
        this.OwnPerformance = OwnPerformance;
    }
}

public static class ResourcePermissionPolicy
{
    public static bool Allows(ResourcePermissionFacts facts, ResourcePermission permission)
    {
        if (!Enum.IsDefined(permission) || !Enum.IsDefined(facts.WorkspaceRole) ||
            !Enum.IsDefined(facts.WorkspaceStatus) || !facts.ActiveMembership ||
            !facts.ResourceInWorkspace || facts.WorkspaceStatus == WorkspaceStatusEnum.Deleted)
            return false;

        var role = facts.Role != 0 ? facts.Role : facts.WorkspaceRole;
        var owner = role == WorkspaceMemberRoleEnum.Owner;
        var workspaceManager = role == WorkspaceMemberRoleEnum.WorkspaceManager;
        var teamManager = owner || workspaceManager || facts.TeamRole == TeamRoleEnum.Manager;
        var teamCreator = facts.TeamRole == TeamRoleEnum.ContentCreator;
        // teamViewerFromTeamRole: only true when the viewer role is explicitly set at team scope
        var teamViewerFromTeamRole = facts.TeamRole == TeamRoleEnum.Viewer;

        if (!facts.TeamRole.HasValue)
        {
            teamManager = teamManager || role == WorkspaceMemberRoleEnum.Manager;
            teamCreator = role == WorkspaceMemberRoleEnum.ContentCreator;
            // Legacy workspace-level Viewer: no analytics access (analytics are team-scoped only)
            teamViewerFromTeamRole = false;
        }

        // Renewal remains possible while normal workspace writes are blocked.
        if (permission == ResourcePermission.BillingManage) return owner;
        var read = permission is ResourcePermission.BrandView or ResourcePermission.ContentView
            or ResourcePermission.ContentViewAllCreators or ResourcePermission.PostView
            or ResourcePermission.SocialView or ResourcePermission.AnalyticsView
            or ResourcePermission.AnalyticsMember;
        if (!read && WorkspaceLifecyclePolicy.IsReadOnly(facts.WorkspaceStatus)) return false;
        if (owner) return true;
        if (!facts.BrandAccessible) return false;

        var contentVisible = teamManager || (teamCreator && (facts.OwnContent || facts.CanViewAllCreators));

        return permission switch
        {
            ResourcePermission.BrandView => true,
            ResourcePermission.BrandManage or ResourcePermission.TeamManage => teamManager,
            ResourcePermission.ContentCreate => teamManager || teamCreator,
            ResourcePermission.ContentView or ResourcePermission.PostView => contentVisible,
            ResourcePermission.ContentViewAllCreators => teamManager || (teamCreator && facts.CanViewAllCreators),
            ResourcePermission.ContentEdit or ResourcePermission.ContentDelete => (teamManager && (facts.SameTeamContent || facts.OwnContent)) || (teamCreator && facts.OwnContent),
            ResourcePermission.ApprovalReview => teamManager || (teamCreator && facts.CanReview),
            ResourcePermission.PostPublish => facts.ChannelAccessible && facts.ChannelCanPublish &&
                (teamManager || (teamCreator && facts.CanPublish && facts.OwnContent)),
            ResourcePermission.SocialView => facts.ChannelAccessible,
            ResourcePermission.SocialManage => teamManager && facts.ChannelAccessible && facts.ChannelCanManage,
            ResourcePermission.AnalyticsView => teamManager || teamCreator || teamViewerFromTeamRole,
            ResourcePermission.AnalyticsMember => teamManager || (teamCreator && facts.OwnPerformance),
            _ => false
        };
    }
}
