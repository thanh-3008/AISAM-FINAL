using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Repositories.IRepositories;
using System.Net;

namespace AISAM.API.Middleware;

public sealed class ActiveProfileMiddleware
{
    private static readonly PathString[] ProtectedPrefixes =
    {
        new("/api/content"),
        new("/api/content-schedules"),
        new("/api/dashboard"),
        new("/api/ai"),
        new("/api/conversations"),
        new("/api/social-auth"),
        new("/api/social"),
        new("/api/posts"),
        new("/api/notifications")
    };

    private readonly RequestDelegate _next;

    public ActiveProfileMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IProfileRepository profileRepository)
    {
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

        if (!Guid.TryParse(context.Request.Headers["X-Profile-Id"], out var profileId))
        {
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, "Missing or invalid X-Profile-Id header.");
            return;
        }

        var userId = UserClaimsHelper.GetUserIdOrThrow(context.User);
        var profile = await profileRepository.GetByIdAsync(profileId, context.RequestAborted);
        if (profile == null)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, "Profile not found.");
            return;
        }

        if (profile.UserId != userId)
        {
            await WriteErrorAsync(context, HttpStatusCode.Forbidden, "You are not allowed to use this profile.");
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
