using AISAM.API.Utils;
using AISAM.Repositories;

namespace AISAM.API.Middleware;

public sealed class ExecutionAttributionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http, AisamContext db)
    {
        db.ExecutionActorId = http.User.Identity?.IsAuthenticated == true
            ? UserClaimsHelper.GetUserIdOrThrow(http.User) : null;
        db.ExecutionIsSystem = false;
        try { await next(http); }
        finally { db.ExecutionActorId = null; db.ExecutionIsSystem = true; }
    }
}
