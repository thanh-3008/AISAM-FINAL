using System.Security.Claims;

namespace AISAM.API.Utils;

public static class UserClaimsHelper
{
    public static Guid GetUserIdOrThrow(ClaimsPrincipal? user)
    {
        var rawId = user?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user?.FindFirstValue("sub");

        if (!Guid.TryParse(rawId, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user context.");
        }

        return userId;
    }
}
