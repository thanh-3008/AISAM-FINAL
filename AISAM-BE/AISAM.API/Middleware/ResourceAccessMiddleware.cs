using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AISAM.API.Middleware;

public sealed class ResourceAccessMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ResourceAccessService access, AisamContext db)
    {
        if (!context.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey, out var value) || value is not WorkspaceMember membership)
        {
            await next(context);
            return;
        }
        var path = (context.Request.Path.Value ?? "").TrimEnd('/').ToLowerInvariant();
        var write = !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method);
        Guid? teamId = null;
        if (context.Request.Headers.TryGetValue("X-Team-Id", out var header))
        {
            if (!Guid.TryParse(header, out var parsed)) { await Deny(context, "Invalid team context."); return; }
            teamId = parsed;
        }
        AISAM.Data.AccessScope scope;
        try { scope = await access.ResolveAsync(membership.WorkspaceId, membership.UserId, write, teamId, context.RequestAborted); }
        catch (UnauthorizedAccessException) { await Deny(context); return; }

        var analytics = path.StartsWith("/api/analytics") || path.StartsWith("/api/workspace-dashboard") ||
            path == "/api/dashboard/summary" || path.EndsWith("/performance");
        var ownEndpoint = path.StartsWith("/api/access/me/") || path.StartsWith("/api/access/content/") && path.EndsWith("/analytics") || path.StartsWith("/api/access/creator-history/");
        var creditHistory = path == "/api/credit-usage" || path == "/api/credit-usage/daily-summary";
        // OQ-007 remains unresolved: Workspace-wide financial/operational details
        // have no approved non-Owner audience. Credit history keeps its own scope.
        var workspaceBilling = path is "/api/credit-usage/wallet" or "/api/quota/workspace/current" or
            "/api/payment/subscription/current" or "/api/payment/history";
        if (workspaceBilling && !scope.IsOwner) { await Deny(context); return; }
        if (analytics && !scope.CanViewAggregate || (creditHistory || ownEndpoint) && scope.Role == WorkspaceMemberRoleEnum.Viewer)
        { await Deny(context); return; }
        if (write && path.StartsWith("/api/posts") && scope.Role is not WorkspaceMemberRoleEnum.Owner and not WorkspaceMemberRoleEnum.Manager)
        { await Deny(context); return; }

        // Explicit filter identifiers must not turn an authorization failure into an empty successful report.
        foreach (var key in new[] { "brandId", "contentId", "campaignId", "integrationId" })
        {
            if (!context.Request.Query.TryGetValue(key, out var raw) || !Guid.TryParse(raw, out var id)) continue;
            var allowed = key switch
            {
                "brandId" => await db.Brands.AnyAsync(b => b.Id == id, context.RequestAborted),
                "contentId" => await db.Contents.AnyAsync(c => c.Id == id, context.RequestAborted),
                "campaignId" => analytics
                    ? await db.CampaignsForAnalytics().AnyAsync(c => c.Id == id, context.RequestAborted)
                    : await db.AdCampaigns.AnyAsync(c => c.Id == id, context.RequestAborted),
                _ => await db.SocialIntegrations.AnyAsync(i => i.Id == id, context.RequestAborted)
            };
            if (!allowed) { await Deny(context); return; }
        }
        await next(context);
    }

    private static async Task Deny(HttpContext context, string message = "You do not have permission to access this resource.")
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(GenericResponse<object>.CreateError(message, HttpStatusCode.Forbidden, "RESOURCE_ACCESS_DENIED"));
    }
}
