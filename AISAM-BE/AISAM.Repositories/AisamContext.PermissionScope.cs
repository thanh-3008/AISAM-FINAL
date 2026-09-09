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
    public bool PermissionManager { get; set; }
    public bool PermissionCreator { get; set; }
    public Guid[] PermissionBrandIds { get; set; } = [];
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
        m.Entity<Brand>().HasQueryFilter(b=>!PermissionScopeEnabled || b.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionBrandIds.Contains(b.Id)));
        m.Entity<Product>().HasQueryFilter(p=>!PermissionScopeEnabled || Brands.Any(b=>b.Id==p.BrandId));
        m.Entity<Team>().HasQueryFilter(t=>!PermissionScopeEnabled || t.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionTeamIds.Contains(t.Id)));
        m.Entity<TeamMember>().HasQueryFilter(t=>!PermissionScopeEnabled || PermissionOwner && Teams.Any(team=>team.Id==t.TeamId) || PermissionTeamIds.Contains(t.TeamId));
        m.Entity<WorkspaceMember>().HasQueryFilter(m=>!PermissionScopeEnabled || m.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || m.UserId==PermissionActorId || PermissionManager && TeamMembers.Any(t=>t.UserId==m.UserId && t.IsActive)));
        m.Entity<CreditUsageRecord>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || c.UserId==PermissionActorId));
        m.Entity<Content>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionBrandIds.Contains(c.BrandId)) &&
            (!PermissionOnlyMyContent || c.PrimaryCreatorId==PermissionActorId) &&
            (!PermissionReviewQueue || PermissionOwner || PermissionManager || PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId)) &&
            (PermissionOwner || PermissionManager || PermissionCreator && (c.PrimaryCreatorId==PermissionActorId || PermissionViewAllBrandIds.Contains(c.BrandId)) || c.Id==PermissionReviewContentId ||
             PermissionReviewQueue && PermissionCreator && PermissionReviewBrandIds.Contains(c.BrandId) && c.Status==AISAM.Data.Enumeration.ContentStatusEnum.PendingApproval));
        m.Entity<SocialIntegration>().HasQueryFilter(i=>!PermissionScopeEnabled || i.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionBrandIds.Contains(i.BrandId)) &&
            (PermissionOwner || PermissionChannelIds.Contains(i.Id)));
        m.Entity<SocialAccount>().HasQueryFilter(a=>!PermissionScopeEnabled || a.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || SocialIntegrations.Any(i=>i.SocialAccountId==a.Id)));
        m.Entity<Post>().HasQueryFilter(p=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==p.ContentId && SocialIntegrations.Any(i=>i.Id==p.IntegrationId && i.BrandId==c.BrandId && i.WorkspaceId==c.WorkspaceId)));
        m.Entity<ContentCalendar>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && Contents.Any(x=>x.Id==c.ContentId));
        m.Entity<Approval>().HasQueryFilter(a=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==a.ContentId));
        m.Entity<AiGeneration>().HasQueryFilter(a=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==a.ContentId));
        m.Entity<AutomationPlan>().HasQueryFilter(p=>!PermissionScopeEnabled || p.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionManager || PermissionCreator && p.CreatedByUserId==PermissionActorId) &&
            (PermissionOwner || PermissionPlanIds.Contains(p.Id)));
        m.Entity<AutomationItem>().HasQueryFilter(i=>!PermissionScopeEnabled || AutomationPlans.Any(p=>p.Id==i.AutomationPlanId) && i.BrandId.HasValue && PermissionBrandIds.Contains(i.BrandId.Value));
        m.Entity<AdCampaign>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || PermissionBrandIds.Contains(c.BrandId)) && (PermissionOwner || PermissionManager));
        m.Entity<AdSet>().HasQueryFilter(a=>!PermissionScopeEnabled || AdCampaigns.Any(c=>c.Id==a.CampaignId));
        m.Entity<Ad>().HasQueryFilter(a=>!PermissionScopeEnabled || AdSets.Any(s=>s.Id==a.AdSetId));
        m.Entity<AdCreative>().HasQueryFilter(a=>!PermissionScopeEnabled || a.ContentId.HasValue && Contents.Any(c=>c.Id==a.ContentId) || Ads.Any(ad=>ad.CreativeId==a.Id));
        m.Entity<CampaignInsightSnapshot>().HasQueryFilter(s=>!PermissionScopeEnabled || s.WorkspaceId==PermissionWorkspaceId && AdCampaigns.Any(c=>c.Id==s.CampaignId));
        m.Entity<PerformanceReport>().HasQueryFilter(r=>!PermissionScopeEnabled || (PermissionOwner || PermissionManager) &&
            (r.PostId.HasValue && Posts.Any(p=>p.Id==r.PostId) || r.AdId.HasValue && Ads.Any(a=>a.Id==r.AdId)));
        m.Entity<VideoGenerationJob>().HasQueryFilter(v=>!PermissionScopeEnabled || v.WorkspaceId==PermissionWorkspaceId && (PermissionOwner || v.UserId==PermissionActorId));
        // Legacy conversations lack creator attribution: do not infer it from workspace membership.
        m.Entity<Conversation>().HasQueryFilter(c=>!PermissionScopeEnabled || c.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || !c.BrandId.HasValue || PermissionBrandIds.Contains(c.BrandId.Value)) && (PermissionOwner || c.CreatedByUserId==PermissionActorId));
        m.Entity<ChatMessage>().HasQueryFilter(c=>!PermissionScopeEnabled || Conversations.Any(x=>x.Id==c.ConversationId));
        m.Entity<Asset>().HasQueryFilter(a=>!PermissionScopeEnabled || a.ExpiredAt==null && a.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || PermissionBrandIds.Contains(a.BrandId??Guid.Empty) && (PermissionManager || a.UploadedBy==PermissionActorId)));
        // Unknown target types are not safe for non-owner broadcast; explicit targets only.
        m.Entity<Notification>().HasQueryFilter(n=>!PermissionScopeEnabled || n.WorkspaceId==PermissionWorkspaceId &&
            (PermissionOwner || n.TargetType=="content" && Contents.Any(c=>c.Id==n.TargetId) ||
             n.TargetType=="post" && Posts.Any(p=>p.Id==n.TargetId) ||
             n.TargetType=="content_schedule" && ContentCalendars.Any(c=>c.Id==n.TargetId)));
    }
}
