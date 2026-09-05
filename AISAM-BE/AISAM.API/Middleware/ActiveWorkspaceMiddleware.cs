using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services;
using System.Net;

namespace AISAM.API.Middleware;

public sealed class ActiveWorkspaceMiddleware
{
    private static readonly PathString[] ProtectedPrefixes =
    {
        new("/api/ai"),
        new("/api/analytics"),
        new("/api/access"),
        new("/api/teams"),
        new("/api/collaboration-tasks"),
        new("/api/brands"),
        new("/api/content"),
        new("/api/content-schedules"),
        new("/api/dashboard"),
        new("/api/products"),
        new("/api/quota"),
        new("/api/workspace-context"),
        new("/api/workspace-members"),
        new("/api/workspace-invitations"),
        new("/api/workspace-dashboard"),
        new("/api/payment"),
        new("/api/posts"),
        new("/api/social"),
        new("/api/social-auth"),
        new("/api/conversations"),
        new("/api/notifications"),
        new("/api/credit-usage"),
        new("/api/campaigns"),
        new("/api/tags"),
        new("/api/automation-plans")
    };

    private readonly RequestDelegate _next;

    public ActiveWorkspaceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IWorkspaceMemberRepository workspaceMemberRepository,
        ISubscriptionRepository subscriptionRepository)
    {
        if (context.Request.Method == HttpMethods.Get &&
            context.Request.Path.Equals("/api/social-auth/facebook/callback", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (context.Request.Method == HttpMethods.Post &&
            (context.Request.Path.Equals("/api/workspace-invitations/accept", StringComparison.OrdinalIgnoreCase) ||
             context.Request.Path.Equals("/api/payment/callback", StringComparison.OrdinalIgnoreCase) ||
             context.Request.Path.Equals("/api/payment/webhook", StringComparison.OrdinalIgnoreCase) ||
             context.Request.Path.Equals("/api/payment/business-workspace-checkout", StringComparison.OrdinalIgnoreCase) ||
             context.Request.Path.Equals("/api/payment/business-workspace-checkout/sync", StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (context.Request.Method == HttpMethods.Get &&
            context.Request.Path.StartsWithSegments("/api/workspace-invitations/validate", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!ProtectedPrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "Authentication is required.");
            return;
        }

        if (!Guid.TryParse(context.Request.Headers["X-Workspace-Id"], out var workspaceId))
        {
            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Missing or invalid X-Workspace-Id header.");
            return;
        }

        var userId = UserClaimsHelper.GetUserIdOrThrow(context.User);
        var membership = await workspaceMemberRepository.GetByWorkspaceAndUserAsync(
            workspaceId,
            userId,
            context.RequestAborted);

        if (membership == null)
        {
            await WriteErrorAsync(context, HttpStatusCode.Forbidden, "You are not a member of this workspace.");
            return;
        }

        if (membership.Workspace.Status == WorkspaceStatusEnum.Deleted)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "Workspace not found.");
            return;
        }

        WorkspaceLifecyclePolicy.SynchronizeStatus(membership.Workspace, DateTime.UtcNow);
        var authorizationError = await ValidateRequestAuthorizationAsync(context, membership, subscriptionRepository);
        if (authorizationError != null)
        {
            await WriteErrorAsync(context, authorizationError.Value.Status, authorizationError.Value.Message, authorizationError.Value.ErrorCode);
            return;
        }

        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = membership;
        await _next(context);
    }

    private static async Task<(HttpStatusCode Status, string Message, string? ErrorCode)?> ValidateRequestAuthorizationAsync(
        HttpContext context,
        WorkspaceMember membership,
        ISubscriptionRepository subscriptionRepository)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;

        if (path.StartsWithSegments("/api/payment"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            return EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageBilling);
        }

        if (WorkspaceLifecyclePolicy.IsReadOnly(membership.Workspace.Status) &&
            method != HttpMethods.Get &&
            !IsOwnerExportRequest(path, membership.Role))
        {
            return (HttpStatusCode.Forbidden, "Workspace is read-only while its subscription is expired.", "WORKSPACE_READ_ONLY");
        }

        var featureError = await EnsureFeatureByRouteAsync(context, membership, subscriptionRepository);
        if (featureError != null)
        {
            return featureError;
        }

        if (path.StartsWithSegments("/api/ai"))
        {
            return EnsurePermission(membership.Role, WorkspacePermissionEnum.GenerateAiContent);
        }

        if (path.StartsWithSegments("/api/automation-plans"))
        {
            if (method == HttpMethods.Get) return null;
            return EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageSchedules);
        }

        if (path.StartsWithSegments("/api/brands"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            return EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageBrands);
        }

        if (path.StartsWithSegments("/api/campaigns"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            return EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageCampaigns);
        }

        if (path.StartsWithSegments("/api/products"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            return EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageProducts);
        }

        if (path.StartsWithSegments("/api/social") || path.StartsWithSegments("/api/social-auth"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            return EnsurePermission(membership.Role, WorkspacePermissionEnum.PublishContent);
        }

        if (path.StartsWithSegments("/api/conversations"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            return EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageContent);
        }

        if (path.StartsWithSegments("/api/content-schedules"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            var permissionError = EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageSchedules);
            if (permissionError != null)
            {
                return permissionError;
            }

            if (method != HttpMethods.Get)
            {
                return await EnsureFeatureAsync(membership.WorkspaceId, subscriptionRepository, membership.Workspace.WorkspaceType, WorkspaceFeatureEnum.SchedulePost);
            }

            return null;
        }

        if (path.StartsWithSegments("/api/content"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            var pathValue = path.Value ?? string.Empty;
            var permission = pathValue.Contains("/publish/", StringComparison.OrdinalIgnoreCase)
                ? WorkspacePermissionEnum.PublishContent
                : pathValue.Contains("/approve", StringComparison.OrdinalIgnoreCase) ||
                  pathValue.Contains("/reject", StringComparison.OrdinalIgnoreCase)
                    ? WorkspacePermissionEnum.ReviewContent
                    : WorkspacePermissionEnum.ManageContent;

            var permissionError = EnsurePermission(membership.Role, permission);
            if (permissionError != null)
            {
                return permissionError;
            }

            if (path.Value?.Contains("/publish/", StringComparison.OrdinalIgnoreCase) == true)
            {
                return await EnsureFeatureAsync(membership.WorkspaceId, subscriptionRepository, membership.Workspace.WorkspaceType, WorkspaceFeatureEnum.MultiPlatformPublish);
            }
        }

        return null;
    }

    private static bool IsOwnerExportRequest(PathString path, WorkspaceMemberRoleEnum role)
    {
        return role == WorkspaceMemberRoleEnum.Owner &&
               path.Value?.Contains("/export", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static async Task<(HttpStatusCode Status, string Message, string? ErrorCode)?> EnsureFeatureByRouteAsync(
        HttpContext context,
        WorkspaceMember membership,
        ISubscriptionRepository subscriptionRepository)
    {
        if (context.Request.Method == HttpMethods.Get &&
            context.Request.Path.StartsWithSegments("/api/content-schedules"))
        {
            return null;
        }

        var feature = ResolveFeature(context.Request.Path);
        if (!feature.HasValue)
        {
            return null;
        }

        return await EnsureFeatureAsync(
            membership.WorkspaceId,
            subscriptionRepository,
            membership.Workspace.WorkspaceType,
            feature.Value);
    }

    private static WorkspaceFeatureEnum? ResolveFeature(PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (path.StartsWithSegments("/api/workspace-dashboard"))
        {
            return WorkspaceFeatureEnum.WorkspaceDashboard;
        }

        if (path.StartsWithSegments("/api/analytics"))
        {
            return WorkspaceFeatureEnum.BasicAnalytics;
        }

        if (path.StartsWithSegments("/api/dashboard"))
        {
            return WorkspaceFeatureEnum.BasicAnalytics;
        }

        if (path.StartsWithSegments("/api/content-schedules"))
        {
            return WorkspaceFeatureEnum.SchedulePost;
        }

        if (path.StartsWithSegments("/api/automation-plans"))
        {
            return WorkspaceFeatureEnum.GenerateText;
        }

        if (path.StartsWithSegments("/api/content") &&
            value.Contains("/publish/", StringComparison.OrdinalIgnoreCase))
        {
            return WorkspaceFeatureEnum.MultiPlatformPublish;
        }

        if (!path.StartsWithSegments("/api/ai"))
        {
            return null;
        }

        if (value.Contains("generate-image", StringComparison.OrdinalIgnoreCase))
        {
            return WorkspaceFeatureEnum.AiImage;
        }

        if (value.Contains("generate-video", StringComparison.OrdinalIgnoreCase))
        {
            return WorkspaceFeatureEnum.AiVideo;
        }

        if (value.Contains("trend", StringComparison.OrdinalIgnoreCase))
        {
            return WorkspaceFeatureEnum.TrendAnalysis;
        }

        if (value.Contains("holiday", StringComparison.OrdinalIgnoreCase))
        {
            return WorkspaceFeatureEnum.HolidaySuggestion;
        }

        if (value.Contains("campaign", StringComparison.OrdinalIgnoreCase))
        {
            return WorkspaceFeatureEnum.CampaignRecommendation;
        }

        return WorkspaceFeatureEnum.GenerateText;
    }

    private static (HttpStatusCode Status, string Message, string? ErrorCode)? EnsurePermission(
        WorkspaceMemberRoleEnum role,
        WorkspacePermissionEnum permission)
    {
        var allowed = permission switch
        {
            WorkspacePermissionEnum.ManageBilling => role == WorkspaceMemberRoleEnum.Owner,
            WorkspacePermissionEnum.ManageBrands => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
            WorkspacePermissionEnum.ManageProducts => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
            WorkspacePermissionEnum.ManageContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager or WorkspaceMemberRoleEnum.ContentCreator,
            WorkspacePermissionEnum.PublishContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
            WorkspacePermissionEnum.ReviewContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
            WorkspacePermissionEnum.GenerateAiContent => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.ContentCreator,
            WorkspacePermissionEnum.ManageSchedules => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
            WorkspacePermissionEnum.ManageCampaigns => role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager,
            _ => false
        };

        return allowed
            ? null
            : (HttpStatusCode.Forbidden, "You do not have permission to perform this action in the active workspace.", "WORKSPACE_PERMISSION_DENIED");
    }

    private static async Task<(HttpStatusCode Status, string Message, string? ErrorCode)?> EnsureFeatureAsync(
        Guid workspaceId,
        ISubscriptionRepository subscriptionRepository,
        WorkspaceTypeEnum workspaceType,
        WorkspaceFeatureEnum feature)
    {
        var subscription = await subscriptionRepository.GetCurrentActiveByWorkspaceIdAsync(workspaceId)
                           ?? new Subscription
                           {
                               Plan = SubscriptionPlanEnum.Free,
                               IsActive = true
                           };

        var enabled = feature switch
        {
            WorkspaceFeatureEnum.GenerateText => true,
            WorkspaceFeatureEnum.MultiPlatformPublish => subscription.Plan is SubscriptionPlanEnum.Plus or SubscriptionPlanEnum.Premium or SubscriptionPlanEnum.PlusTrial,
            WorkspaceFeatureEnum.SchedulePost => subscription.Plan is SubscriptionPlanEnum.Plus or SubscriptionPlanEnum.Premium or SubscriptionPlanEnum.PlusTrial,
            WorkspaceFeatureEnum.AiImage => subscription.Plan is SubscriptionPlanEnum.Plus or SubscriptionPlanEnum.Premium or SubscriptionPlanEnum.PlusTrial,
            WorkspaceFeatureEnum.AiVideo => subscription.Plan is SubscriptionPlanEnum.Premium,
            WorkspaceFeatureEnum.TrendAnalysis => subscription.Plan is SubscriptionPlanEnum.Premium,
            WorkspaceFeatureEnum.HolidaySuggestion => subscription.Plan is SubscriptionPlanEnum.Premium,
            WorkspaceFeatureEnum.CampaignRecommendation => subscription.Plan is SubscriptionPlanEnum.Premium,
            WorkspaceFeatureEnum.BasicAnalytics => true,
            WorkspaceFeatureEnum.AdvancedAnalytics => subscription.Plan is SubscriptionPlanEnum.Premium,
            WorkspaceFeatureEnum.WorkspaceDashboard => subscription.Plan is not SubscriptionPlanEnum.Free,
            _ => false
        };

        return enabled
            ? null
            : (HttpStatusCode.Forbidden, "The active subscription plan does not include this feature.", "WORKSPACE_FEATURE_NOT_AVAILABLE");
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message, string? errorCode = null)
    {
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(GenericResponse<object>.CreateError(message, status, errorCode));
    }
}
