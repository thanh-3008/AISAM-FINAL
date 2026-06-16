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
        var membership = WorkspaceContextHelper.GetActiveWorkspaceMembershipOrThrow(context);
        var existingProfile = (await profileRepository.GetByUserIdAsync(membership.UserId, cancellationToken))
            .FirstOrDefault();

        if (existingProfile != null)
        {
            return existingProfile.Id;
        }

        var profile = await profileRepository.CreateAsync(new Profile
        {
            UserId = membership.UserId,
            Name = $"{membership.Workspace.Name} Legacy Profile",
            ProfileType = ProfileTypeEnum.Free,
            Status = ProfileStatusEnum.Pending
        }, cancellationToken);

        return profile.Id;
    }
}
