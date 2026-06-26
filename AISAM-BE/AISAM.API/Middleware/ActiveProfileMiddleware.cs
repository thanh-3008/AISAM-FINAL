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

        if (!Guid.TryParse(context.Request.Headers["X-Profile-Id"], out var profileId))
        {
            var userProfiles = await profileRepository.GetByUserIdAsync(userId, context.RequestAborted);
            var workspaceHeader = context.Request.Headers["X-Workspace-Id"].FirstOrDefault();
            Profile? profile = null;

            if (Guid.TryParse(workspaceHeader, out var wsId))
            {
                profile = userProfiles.FirstOrDefault(p => p.Id == wsId);
            }

            profile ??= userProfiles.FirstOrDefault(p => p.Status == ProfileStatusEnum.Active);

            if (profile == null && Guid.TryParse(workspaceHeader, out var newWsId))
            {
                var workspace = await workspaceRepository.GetByIdAsync(newWsId, context.RequestAborted);
                var user = await userRepository.GetByIdAsync(userId);
                profile = await profileRepository.CreateAsync(new Profile
                {
                    UserId = userId,
                    Name = $"{user?.FullName ?? "User"}'s Profile",
                    ProfileType = ProfileTypeEnum.Free,
                    Status = ProfileStatusEnum.Active
                }, context.RequestAborted);
            }

            if (profile != null)
            {
                context.Items[ProfileContextHelper.ActiveProfileItemKey] = profile.Id;
                await _next(context);
                return;
            }

            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "Missing or invalid X-Profile-Id header.");
            return;
        }

        var resolvedProfile = await profileRepository.GetByIdAsync(profileId, context.RequestAborted);
        if (resolvedProfile == null)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "Profile not found.");
            return;
        }

        if (resolvedProfile.UserId != userId)
        {
            await WriteErrorAsync(context, HttpStatusCode.Forbidden, "You are not allowed to use this profile.");
            return;
        }

        if (context.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceItemKey, out var activeWorkspaceObj) &&
            activeWorkspaceObj is Guid activeWorkspaceId &&
            resolvedProfile.Id != activeWorkspaceId)
        {
            await WriteErrorAsync(context, HttpStatusCode.Forbidden, "Profile does not belong to active workspace.");
            return;
        }

        context.Items[ProfileContextHelper.ActiveProfileItemKey] = resolvedProfile.Id;
        await _next(context);
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(GenericResponse<object>.CreateError(message, status));
    }
}
