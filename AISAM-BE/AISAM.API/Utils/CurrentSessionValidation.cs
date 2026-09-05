using AISAM.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AISAM.API.Utils;

public static class CurrentSessionValidation
{
    public static async Task ValidateAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        if (!Guid.TryParse(principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) ||
            !Guid.TryParse(principal?.FindFirst("sid")?.Value, out var sessionId))
        {
            context.Fail("A current session is required.");
            return;
        }
        var db = context.HttpContext.RequestServices.GetRequiredService<AisamContext>();
        var now = DateTime.UtcNow;
        var role = principal?.FindFirst(ClaimTypes.Role)?.Value;
        var active = await db.Sessions.AnyAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive &&
            s.RevokedAt == null && s.ExpiresAt > now && s.User.IsActive && s.User.Role.ToString() == role,
            context.HttpContext.RequestAborted);
        if (!active) context.Fail("Session or role is no longer valid.");
    }
}
