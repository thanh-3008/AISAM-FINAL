namespace AISAM.API.Utils;

public static class ProfileContextHelper
{
    public const string ActiveProfileItemKey = "ActiveProfileId";

    public static Guid GetActiveProfileIdOrThrow(HttpContext context)
    {
        if (context.Items.TryGetValue(ActiveProfileItemKey, out var value) &&
            value is Guid profileId)
        {
            return profileId;
        }

        throw new InvalidOperationException("Invalid profile context.");
    }
}
