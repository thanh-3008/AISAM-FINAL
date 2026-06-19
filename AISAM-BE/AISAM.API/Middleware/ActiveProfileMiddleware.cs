using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using System.Net;

namespace AISAM.API.Middleware;

public sealed class ActiveProfileMiddleware
{
    private static readonly PathString[] ProtectedPrefixes =
    {
        new("/api/dev/scheduler")
    };

    private readonly RequestDelegate _next;

    public ActiveProfileMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IProfileRepository profileRepository, IWebHostEnvironment environment)
    {
        if (context.Request.Path.StartsWithSegments("/api/dev/scheduler") && !environment.IsDevelopment())
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

        Profile? profile = null;
        if (context.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey, out var membershipValue) &&
            membershipValue is WorkspaceMember membership)
        {
            profile = await profileRepository.GetByWorkspaceIdAsync(membership.WorkspaceId, context.RequestAborted);
            if (profile == null)
            {
                profile = await profileRepository.CreateAsync(new Profile
                {
                    UserId = membership.UserId,
                    WorkspaceId = membership.WorkspaceId,
                    Name = string.IsNullOrWhiteSpace(membership.Workspace.Name)
                        ? "Workspace Profile"
                        : membership.Workspace.Name,
                    ProfileType = ProfileTypeEnum.Free,
                    Status = ProfileStatusEnum.Pending
                }, context.RequestAborted);
            }
        }
        else
        {
            var userId = UserClaimsHelper.GetUserIdOrThrow(context.User);
            profile = await profileRepository.GetFirstByUserIdAsync(userId, context.RequestAborted);
            if (profile?.UserId != userId)
            {
                await WriteErrorAsync(context, HttpStatusCode.Forbidden, "You are not allowed to use this profile.");
                return;
            }
        }

        if (profile == null)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "No profile found for this workspace.");
            return;
        }

        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profile.Id;
        await _next(context);
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(GenericResponse<object>.CreateError(message, status));
    }
}
