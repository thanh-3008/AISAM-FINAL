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
        new("/api/dev/scheduler"),
        new("/api/ai"),
        new("/api/conversations"),
    };

    private readonly RequestDelegate _next;

    public ActiveProfileMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IProfileRepository profileRepository,
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        IWebHostEnvironment environment)
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

        var userId = UserClaimsHelper.GetUserIdOrThrow(context.User);
        Profile? profile = null;

        // 1. First, try to get profile from Workspace Membership
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
                    Status = ProfileStatusEnum.Active
                }, context.RequestAborted);
            }
        }
        // 2. Fallback: try to get from X-Profile-Id header
        else if (Guid.TryParse(context.Request.Headers["X-Profile-Id"], out var profileId))
        {
            profile = await profileRepository.GetByIdAsync(profileId, context.RequestAborted);
            if (profile != null && profile.UserId != userId)
            {
                await WriteErrorAsync(context, HttpStatusCode.Forbidden, "You are not allowed to use this profile.");
                return;
            }
        }
        // 3. Fallback: find matching profile or create if needed
        else
        {
            var userProfiles = await profileRepository.GetByUserIdAsync(userId, context.RequestAborted);
            var workspaceHeader = context.Request.Headers["X-Workspace-Id"].FirstOrDefault();
            
            if (Guid.TryParse(workspaceHeader, out var wsId))
            {
                profile = userProfiles.FirstOrDefault(p => p.WorkspaceId == wsId || p.Id == wsId);
            }
            
            profile ??= userProfiles.FirstOrDefault(p => p.Status == ProfileStatusEnum.Active) ?? userProfiles.FirstOrDefault();

            if (profile == null && Guid.TryParse(workspaceHeader, out var newWsId))
            {
                var workspace = await workspaceRepository.GetByIdAsync(newWsId, context.RequestAborted);
                var user = await userRepository.GetByIdAsync(userId);
                profile = await profileRepository.CreateAsync(new Profile
                {
                    UserId = userId,
                    WorkspaceId = newWsId,
                    Name = $"{user?.FullName ?? "User"}'s Profile",
                    ProfileType = ProfileTypeEnum.Free,
                    Status = ProfileStatusEnum.Active
                }, context.RequestAborted);
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
