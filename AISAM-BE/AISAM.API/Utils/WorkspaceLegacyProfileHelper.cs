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

        if (context.Items.TryGetValue(WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey, out var membershipValue) &&
            membershipValue is WorkspaceMember membership)
        {
            var workspaceProfile = await profileRepository.GetByWorkspaceIdAsync(membership.WorkspaceId, cancellationToken);
            if (workspaceProfile != null)
            {
                context.Items[ProfileContextHelper.ActiveProfileItemKey] = workspaceProfile.Id;
                return workspaceProfile.Id;
            }

            workspaceProfile = await profileRepository.CreateAsync(new Profile
            {
                UserId = membership.UserId,
                WorkspaceId = membership.WorkspaceId,
                Name = string.IsNullOrWhiteSpace(membership.Workspace.Name)
                    ? "Workspace Profile"
                    : membership.Workspace.Name,
                ProfileType = ProfileTypeEnum.Free,
                Status = ProfileStatusEnum.Pending
            }, cancellationToken);

            context.Items[ProfileContextHelper.ActiveProfileItemKey] = workspaceProfile.Id;
            return workspaceProfile.Id;
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
