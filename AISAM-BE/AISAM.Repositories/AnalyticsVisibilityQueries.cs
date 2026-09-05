using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories;

public partial class AisamContext
{
    public IQueryable<PerformanceReport> ContentAnalyticsReports()
    {
        if (!AccessScope.Enforced) throw new UnauthorizedAccessException("A current workspace scope is required.");
        if (!AccessScope.IsCreator) return PerformanceReports;
        // Historical metrics are content-specific: bypass unrelated current Brand/Channel
        // navigation filters, then reapply BOTH workspace boundaries and the history scope.
        return PerformanceReports.IgnoreQueryFilters().Where(r => r.Post != null &&
            r.Post.Content.WorkspaceId == AccessScope.WorkspaceId && r.Post.Integration.WorkspaceId == AccessScope.WorkspaceId &&
            AccessScope.HistoricalContentIds.Contains(r.Post.ContentId));
    }

    public IQueryable<AdCampaign> CampaignsForAnalytics()
        => AdCampaigns.Where(c => !AccessScope.Enforced || AccessScope.IsOwner ||
            AccessScope.CanViewAggregate && AccessScope.AnalyticsCampaignIds.Contains(c.Id));

    public IQueryable<AutomationItem> AutomationItemsForAnalytics(Guid workspaceId)
        => AutomationItems.Where(item => item.AutomationPlan.WorkspaceId == workspaceId &&
            (!AccessScope.Enforced || AccessScope.WorkspaceId == workspaceId &&
                (AccessScope.IsOwner || AccessScope.CanViewAggregate && item.BrandId.HasValue &&
                    AccessScope.BrandIds.Contains(item.BrandId.Value) && item.ContentCalendarId.HasValue &&
                    ContentCalendars.Any(c => c.Id == item.ContentCalendarId && c.WorkspaceId == workspaceId &&
                        c.ContentId == item.ContentId && c.IntegrationId.HasValue && AccessScope.IntegrationIds.Contains(c.IntegrationId.Value)))));

    public IQueryable<AdCampaignResponseDto> CampaignMetadata(Guid workspaceId)
    {
        if (!AccessScope.Enforced || AccessScope.WorkspaceId != workspaceId)
            throw new UnauthorizedAccessException("A current workspace scope is required.");
        return AdCampaigns.AsNoTracking().Where(c => c.WorkspaceId == workspaceId).Select(c => new AdCampaignResponseDto
        {
            Id = c.Id, ProfileId = c.ProfileId, WorkspaceId = c.WorkspaceId, BrandId = c.BrandId,
            BrandName = c.Brand.Name, ProductId = c.ProductId, ProductName = c.Product == null ? null : c.Product.Name,
            ContentId = c.ContentId, ContentTitle = c.Content == null ? null : c.Content.Title,
            Targeting = c.Targeting, AdAccountId = c.AdAccountId, AdAccountCurrency = c.AdAccountCurrency,
            FacebookCampaignId = c.FacebookCampaignId, Platform = c.Platform, Name = c.Name, Objective = c.Objective,
            Budget = c.Budget, StartDate = c.StartDate, EndDate = c.EndDate, IsActive = c.IsActive, IsDeleted = c.IsDeleted,
            Status = c.Status, LandingUrl = c.LandingUrl, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt,
            DeploymentStatus = c.DeploymentStatus, DeploymentStep = c.DeploymentStep, DeploymentMessage = c.DeploymentMessage,
            CanViewAnalytics = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(c.Id),
            Impressions = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(c.Id) ? c.Impressions : null,
            Clicks = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(c.Id) ? c.Clicks : null,
            Spend = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(c.Id) ? c.Spend : null,
            Conversions = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(c.Id) ? c.Conversions : null,
            AdSets = c.AdSets.Where(s => !s.IsDeleted).Select(s => new AdSetSummaryDto
            {
                Id = s.Id, Name = s.Name, FacebookAdSetId = s.FacebookAdSetId, DailyBudget = s.DailyBudget, Status = s.Status,
                Impressions = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(s.CampaignId) ? s.Campaign.Impressions / s.Campaign.AdSets.Count(a => !a.IsDeleted) : null,
                Clicks = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(s.CampaignId) ? s.Campaign.Clicks / s.Campaign.AdSets.Count(a => !a.IsDeleted) : null,
                Spend = AccessScope.IsOwner || AccessScope.AnalyticsCampaignIds.Contains(s.CampaignId) ? s.Campaign.Spend / s.Campaign.AdSets.Count(a => !a.IsDeleted) : null,
                Ads = s.Ads.Where(a => !a.IsDeleted).Select(a => new AdSummaryDto
                {
                    Id = a.Id, AdId = a.AdId, Status = a.Status,
                    CreativeId = a.Creative == null ? null : a.Creative.CreativeId,
                    CallToAction = a.Creative == null ? null : a.Creative.CallToAction,
                    LinkUrl = a.Creative == null ? null : a.Creative.LinkUrl
                }).ToList()
            }).ToList()
        });
    }
}
