using AISAM.Repositories.IRepositories;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace AISAM.API.Middleware
{
    public class MaintenanceModeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        private static readonly PathString[] AdminPaths = { new("/api/admin"), new("/api/auth") };

        public MaintenanceModeMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, ISystemSettingRepository settingRepo)
        {
            var isMaintenance = await _cache.GetOrCreateAsync("system.maintenance_mode", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                var setting = await settingRepo.GetByKeyAsync("system.maintenance_mode");
                if (setting != null && bool.TryParse(setting.Value.Trim('"'), out var val))
                    return val;
                return false;
            });

            if (isMaintenance)
            {
                var path = context.Request.Path;
                var isAdminRoute = AdminPaths.Any(p => path.StartsWithSegments(p));

                if (!isAdminRoute)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"success\":false,\"message\":\"System is under maintenance. Please try again later.\",\"statusCode\":503}");
                    return;
                }
            }

            await _next(context);
        }
    }
}
