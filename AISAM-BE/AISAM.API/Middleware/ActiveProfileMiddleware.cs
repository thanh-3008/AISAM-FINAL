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
        IUserRepository userRepository,
        IWebHostEnvironment environment)
    {
        if (context.Request.Path.StartsWithSegments("/api/dev/scheduler") && !environment.IsDevelopment())
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

        var userId = UserClaimsHelper.GetUserIdOrThrow(context.User);

        if (!Guid.TryParse(context.Request.Headers["X-Profile-Id"], out var profileId))
        {
            var userProfiles = await profileRepository.GetByUserIdAsync(userId, context.RequestAborted);
            // Workspace members may legitimately have only a legacy Pending
            // profile. The profile is a data-owner key here, not an
            // authentication credential, so its lifecycle status must not turn
            // an authenticated workspace request into a 401.
            var profile = userProfiles.FirstOrDefault(p => p.Status == ProfileStatusEnum.Active)
                          ?? userProfiles.FirstOrDefault();

            if (profile == null && !userProfiles.Any())
            {
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

            await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Missing or invalid X-Profile-Id header.");
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

        context.Items[ProfileContextHelper.ActiveProfileItemKey] = resolvedProfile.Id;
        await _next(context);
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsJsonAsync(GenericResponse<object>.CreateError(message, status));
    }
}
