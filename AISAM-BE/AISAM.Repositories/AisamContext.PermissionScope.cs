using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    // Populated from persisted membership/assignments before any business query.
    public bool PermissionScopeEnabled { get; set; }
    public Guid PermissionWorkspaceId { get; set; }
    public Guid PermissionActorId { get; set; }
    public bool PermissionOwner { get; set; }
    public bool PermissionWorkspaceManager { get; set; }
    public bool PermissionManager { get; set; }
    public bool PermissionCreator { get; set; }
    public bool PermissionViewer { get; set; }
    public Guid[] PermissionBrandIds { get; set; } = [];
    public Guid[] PermissionManagerBrandIds { get; set; } = [];
    public Guid[] PermissionChannelIds { get; set; } = [];
    public Guid[] PermissionViewAllBrandIds { get; set; } = [];
    public Guid[] PermissionPlanIds { get; set; } = [];
    public Guid[] PermissionTeamIds { get; set; } = [];
    public Guid[] PermissionReviewBrandIds { get; set; } = [];
    public bool PermissionReviewQueue { get; set; }
    public bool PermissionOnlyMyContent { get; set; }
    public Guid? PermissionReviewContentId { get; set; }
    public Func<object,EntityState,CancellationToken,Task>? BeforePermissionMutation { get; set; }

    private void ConfigurePermissionScope(ModelBuilder m)
    {
        m.Entity<Brand>().HasQueryFilter(b=>!PermissionScopeEnabled || b.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionWorkspaceManager || PermissionBrandIds.Contains(b.Id)));
        m.Entity<Product>().HasQueryFilter(p=>!PermissionScopeEnabled || Brands.Any(b=>b.Id==p.BrandId));
        m.Entity<Team>().HasQueryFilter(t=>!PermissionScopeEnabled || t.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionWorkspaceManager || PermissionTeamIds.Contains(t.Id)));
        m.Entity<TeamMember>().HasQueryFilter(t=>!PermissionScopeEnabled || (PermissionOwner || PermissionWorkspaceManager) && Teams.Any(team=>team.Id==t.TeamId) || PermissionTeamIds.Contains(t.TeamId));
        m.Entity<WorkspaceMember>().HasQueryFilter(m=>!PermissionScopeEnabled || m.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || m.UserId==PermissionActorId ||
             (PermissionManager && TeamMembers.Any(t=>t.UserId==m.UserId && t.IsActive)) ||
             TeamMembers.Any(t=>t.UserId==m.UserId && PermissionTeamIds.Contains(t.TeamId) && t.IsActive)));
        m.Entity<CreditUsageRecord>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionWorkspaceManager || c.UserId==PermissionActorId));
        m.Entity<Content>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || PermissionBrandIds.Contains(c.BrandId)) &&
            (PermissionOwner || PermissionWorkspaceManager || !c.TeamId.HasValue || PermissionTeamIds.Contains(c.TeamId.Value)) &&
            (!PermissionOnlyMyContent || c.PrimaryCreatorId==PermissionActorId) &&
            (!PermissionReviewQueue || PermissionOwner || PermissionWorkspaceManager || PermissionManager || (PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId))) &&
            (PermissionOwner || PermissionWorkspaceManager || PermissionManager || PermissionManagerBrandIds.Contains(c.BrandId) ||
             (PermissionCreator && (c.PrimaryCreatorId==PermissionActorId || PermissionViewAllBrandIds.Contains(c.BrandId))) ||
             c.Id==PermissionReviewContentId ||
             (PermissionReviewQueue && PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId) && c.Status==AISAM.Data.Enumeration.ContentStatusEnum.PendingApproval) ||
             (c.Status==AISAM.Data.Enumeration.ContentStatusEnum.Approved || c.Status==AISAM.Data.Enumeration.ContentStatusEnum.Published)));
        m.Entity<SocialIntegration>().HasQueryFilter(i=>!PermissionScopeEnabled || i.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || PermissionBrandIds.Contains(i.BrandId)) &&
            (PermissionOwner || PermissionWorkspaceManager || PermissionChannelIds.Contains(i.Id)));
        m.Entity<SocialAccount>().HasQueryFilter(a=>!PermissionScopeEnabled || a.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || SocialIntegrations.Any(i=>i.SocialAccountId==a.Id)));
        m.Entity<Post>().HasQueryFilter(p=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==p.ContentId && c.WorkspaceId==PermissionWorkspaceId &&
            (c.PrimaryCreatorId==PermissionActorId ||
             SocialIntegrations.Any(i=>i.Id==p.IntegrationId && i.BrandId==c.BrandId && i.WorkspaceId==PermissionWorkspaceId &&
                 (PermissionOwner || PermissionWorkspaceManager || PermissionChannelIds.Contains(i.Id))))));
        m.Entity<ContentCalendar>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && Contents.Any(x=>x.Id==c.ContentId));
        m.Entity<Approval>().HasQueryFilter(a=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==a.ContentId));
        m.Entity<AiGeneration>().HasQueryFilter(a=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==a.ContentId));
        m.Entity<AutomationPlan>().HasQueryFilter(p=>!PermissionScopeEnabled || p.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || PermissionManager || (PermissionCreator && p.CreatedByUserId==PermissionActorId)) &&
            (PermissionOwner || PermissionWorkspaceManager || PermissionPlanIds.Contains(p.Id)));
        m.Entity<AutomationItem>().HasQueryFilter(i=>!PermissionScopeEnabled || (PermissionOwner || PermissionWorkspaceManager || AutomationPlans.Any(p=>p.Id==i.AutomationPlanId) && (!i.BrandId.HasValue || PermissionBrandIds.Contains(i.BrandId.Value))));
        m.Entity<AdCampaign>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionWorkspaceManager || PermissionBrandIds.Contains(c.BrandId)) && (PermissionOwner || PermissionWorkspaceManager || PermissionManager));
        m.Entity<AdSet>().HasQueryFilter(a=>!PermissionScopeEnabled || AdCampaigns.Any(c=>c.Id==a.CampaignId));
        m.Entity<Ad>().HasQueryFilter(a=>!PermissionScopeEnabled || AdSets.Any(s=>s.Id==a.AdSetId));
        m.Entity<AdCreative>().HasQueryFilter(a=>!PermissionScopeEnabled || a.ContentId.HasValue && Contents.Any(c=>c.Id==a.ContentId) || Ads.Any(ad=>ad.CreativeId==a.Id));
        m.Entity<CampaignInsightSnapshot>().HasQueryFilter(s=>!PermissionScopeEnabled || s.WorkspaceId==PermissionWorkspaceId && AdCampaigns.Any(c=>c.Id==s.CampaignId));
        m.Entity<PerformanceReport>().HasQueryFilter(r=>!PermissionScopeEnabled ||
            ((PermissionOwner || PermissionWorkspaceManager || PermissionManager || PermissionCreator || PermissionViewer) &&
                r.PostId.HasValue && Posts.Any(p=>p.Id==r.PostId)) ||
            ((PermissionOwner || PermissionWorkspaceManager || PermissionManager) &&
                r.AdId.HasValue && Ads.Any(a=>a.Id==r.AdId)));
        m.Entity<VideoGenerationJob>().HasQueryFilter(v=>!PermissionScopeEnabled || v.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionWorkspaceManager || v.UserId==PermissionActorId));
        // Legacy conversations lack creator attribution: do not infer it from workspace membership.
        m.Entity<Conversation>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || !c.BrandId.HasValue || PermissionBrandIds.Contains(c.BrandId.Value)) &&
            (PermissionOwner || PermissionWorkspaceManager || c.CreatedByUserId==PermissionActorId));
        m.Entity<ChatMessage>().HasQueryFilter(c=>!PermissionScopeEnabled || Conversations.Any(x=>x.Id==c.ConversationId));
        m.Entity<Asset>().HasQueryFilter(a=>!PermissionScopeEnabled || a.ExpiredAt==null && a.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || !a.BrandId.HasValue || (PermissionBrandIds.Contains(a.BrandId.Value) && (PermissionManager || a.UploadedBy==PermissionActorId))));
        // Unknown target types are not safe for non-owner broadcast; explicit targets only.
        m.Entity<Notification>().HasQueryFilter(n=>!PermissionScopeEnabled || n.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionWorkspaceManager || n.TargetType=="content" && Contents.Any(c=>c.Id==n.TargetId) ||
             n.TargetType=="post" && Posts.Any(p=>p.Id==n.TargetId) ||
             n.TargetType=="content_schedule" && ContentCalendars.Any(c=>c.Id==n.TargetId)));
    }
}
