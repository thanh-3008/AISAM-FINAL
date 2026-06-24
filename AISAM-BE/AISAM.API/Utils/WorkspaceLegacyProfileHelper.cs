using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;

namespace AISAM.API.Utils;

public static class WorkspaceLegacyProfileHelper
{
    public static async Task<Guid> GetOrCreateProfileIdAsync(
        HttpContext context,
        IProfileRepository profileRepository,
        CancellationToken cancellationToken = default)
    {
        if (context.Items.TryGetValue(ProfileContextHelper.ActiveProfileItemKey, out var value) &&
            value is Guid activeProfileId)
        {
            return activeProfileId;
        }

        var userId = UserClaimsHelper.GetUserIdOrThrow(context.User);
        var profile = (await profileRepository.GetByUserIdAsync(userId, cancellationToken)).FirstOrDefault();
        if (profile != null)
        {
            context.Items[ProfileContextHelper.ActiveProfileItemKey] = profile.Id;
            return profile.Id;
        }

        profile = await profileRepository.CreateAsync(new Profile
        {
            UserId = userId,
            Name = "Workspace Profile",
            ProfileType = ProfileTypeEnum.Free,
            Status = ProfileStatusEnum.Pending
        }, cancellationToken);

        context.Items[ProfileContextHelper.ActiveProfileItemKey] = profile.Id;
        return profile.Id;
    }
}
