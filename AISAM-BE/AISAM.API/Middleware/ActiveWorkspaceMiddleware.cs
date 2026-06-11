using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using System.Net;

namespace AISAM.API.Middleware;

public sealed class ActiveWorkspaceMiddleware
{
    private static readonly PathString[] ProtectedPrefixes =
    {
        new("/api/workspace-context"),
        new("/api/workspace-members"),
        new("/api/workspace-invitations"),
        new("/api/workspace-dashboard")
    };

    private readonly RequestDelegate _next;

    public ActiveWorkspaceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IWorkspaceMemberRepository workspaceMemberRepository)
    {
        if (context.Request.Method == HttpMethods.Post &&
            context.Request.Path.Equals("/api/workspace-invitations/accept", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!ProtectedPrefixes.Any(prefix => context.Request.Path.StartsWithSegments(prefix)))
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
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "Missing or invalid X-Workspace-Id header.");
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

        context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey] = workspaceId;
        context.Items[WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] = membership;
        await _next(context);
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(GenericResponse<object>.CreateError(message, status));
    }
}
