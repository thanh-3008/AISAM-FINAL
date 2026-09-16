using AISAM.Data.Enumeration;

namespace AISAM.Services.Access;

public enum RbacV2Action
{
    BillingRead, BillingManage, BrandRead, BrandManage, TeamManage, SocialRead, SocialManage,
    ContentRead, ContentCreate, ContentEdit, ContentDelete, ApprovalReview, Publish,
    AnalyticsRead, MemberPerformance, ProviderAnalytics, ApprovalWithdraw
}

// Server-resolved facts, never accepted as a request body. TeamRole is for the target Team only.
public sealed record RbacV2Facts(
    WorkspaceRoleV2? WorkspaceRole, WorkspaceStatusEnum WorkspaceStatus,
    bool ActiveMembership, bool SameWorkspace, bool BrandLinked,
    TeamRoleEnum? TeamRole, bool TeamActive, bool ChannelGranted,
    bool ChannelActive, ContentStatusEnum? ContentStatus, bool OwnContent, bool OwnPerformance);

public static class RbacV2Policy
{
    public static bool Allows(RbacV2Facts f, RbacV2Action action)
    {
        if (!Enum.IsDefined(action) || f.WorkspaceRole is not { } role || !Enum.IsDefined(role) ||
            !f.ActiveMembership || !f.SameWorkspace || !Enum.IsDefined(f.WorkspaceStatus) ||
            f.WorkspaceStatus==WorkspaceStatusEnum.Deleted) return false;
        var owner=role==WorkspaceRoleV2.Owner;
        var admin=owner || role==WorkspaceRoleV2.WorkspaceManager;
        if(action==RbacV2Action.BillingRead) return admin;
        if(action==RbacV2Action.BillingManage) return owner;
        var read=action is RbacV2Action.BrandRead or RbacV2Action.SocialRead or RbacV2Action.ContentRead
            or RbacV2Action.AnalyticsRead or RbacV2Action.MemberPerformance or RbacV2Action.ProviderAnalytics;
        if(!read && WorkspaceLifecyclePolicy.IsReadOnly(f.WorkspaceStatus)) return false;
        if(action is RbacV2Action.BrandManage or RbacV2Action.TeamManage or RbacV2Action.SocialManage) return admin;
        if(!admin && (!f.TeamActive || !f.BrandLinked || f.TeamRole is not { } tr || !Enum.IsDefined(tr))) return false;
        var manager=f.TeamRole==TeamRoleEnum.Manager;
        var creator=f.TeamRole==TeamRoleEnum.ContentCreator;
        return action switch
        {
            RbacV2Action.BrandRead => true,
            RbacV2Action.SocialRead => admin || f.ChannelGranted,
            RbacV2Action.ContentRead => admin || manager || creator || f.ContentStatus is ContentStatusEnum.Approved or ContentStatusEnum.Published,
            RbacV2Action.ContentCreate => f.TeamActive && f.BrandLinked && (admin || manager || creator),
            RbacV2Action.ContentEdit or RbacV2Action.ContentDelete =>
                f.ContentStatus is ContentStatusEnum.Draft or ContentStatusEnum.Rejected && (admin || manager || creator && f.OwnContent),
            RbacV2Action.ApprovalReview => f.ContentStatus==ContentStatusEnum.PendingApproval && (admin || manager),
            RbacV2Action.ApprovalWithdraw => f.ContentStatus==ContentStatusEnum.Approved && (admin || manager),
            RbacV2Action.Publish => f.TeamActive && f.BrandLinked && f.ChannelActive && f.ContentStatus is ContentStatusEnum.Approved or ContentStatusEnum.Published && (admin || manager && f.ChannelGranted),
            RbacV2Action.AnalyticsRead => true,
            RbacV2Action.MemberPerformance => admin || manager || creator && f.OwnPerformance,
            RbacV2Action.ProviderAnalytics => admin || manager && f.ChannelGranted,
            _ => false
        };
    }
}
