using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using System.Net;

namespace AISAM.Services.Service
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IUserRepository _userRepository;

        public ProfileService(IProfileRepository profileRepository, IUserRepository userRepository)
        {
            _profileRepository = profileRepository;
            _userRepository = userRepository;
        }

        public async Task<GenericResponse<ProfileResponseDto>> GetProfileByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var profile = await _profileRepository.GetByIdAsync(id, cancellationToken);
            if (profile == null || profile.UserId != userId)
            {
                return GenericResponse<ProfileResponseDto>.CreateError("Profile not found", HttpStatusCode.NotFound);
            }

            return GenericResponse<ProfileResponseDto>.CreateSuccess(MapToDto(profile), "Profile retrieved successfully");
        }

        public async Task<GenericResponse<IEnumerable<ProfileResponseDto>>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return GenericResponse<IEnumerable<ProfileResponseDto>>.CreateError("User not found");
            }

            var profiles = await _profileRepository.SearchUserProfilesAsync(userId, searchTerm, isDeleted, cancellationToken);
            var data = profiles.Select(MapToDto);

            return GenericResponse<IEnumerable<ProfileResponseDto>>.CreateSuccess(data, "Profiles retrieved successfully");
        }

        public async Task<GenericResponse<ProfileResponseDto>> CreateProfileAsync(Guid userId, CreateProfileRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return GenericResponse<ProfileResponseDto>.CreateError("User not found");
            }

            if (request.AvatarFile != null)
            {
                return GenericResponse<ProfileResponseDto>.CreateError("Avatar file upload is not enabled in the current MVP backend. Use AvatarUrl instead.");
            }

            var profile = new Profile
            {
                UserId = userId,
                Name = request.Name,
                ProfileType = request.ProfileType,
                CompanyName = request.CompanyName,
                Bio = request.Bio,
                AvatarUrl = request.AvatarUrl
            };

            var createdProfile = await _profileRepository.CreateAsync(profile, cancellationToken);

            return GenericResponse<ProfileResponseDto>.CreateSuccess(MapToDto(createdProfile), "Profile created successfully");
        }

        public async Task<GenericResponse<ProfileResponseDto>> UpdateProfileAsync(Guid id, Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            var profile = await _profileRepository.GetByIdAsync(id, cancellationToken);
            if (profile == null || profile.UserId != userId)
            {
                return GenericResponse<ProfileResponseDto>.CreateError("Profile not found", HttpStatusCode.NotFound);
            }

            if (request.AvatarFile != null)
            {
                return GenericResponse<ProfileResponseDto>.CreateError("Avatar file upload is not enabled in the current MVP backend. Use AvatarUrl instead.");
            }

            if (request.Name != null)
            {
                profile.Name = request.Name;
            }

            if (request.ProfileType.HasValue)
            {
                profile.ProfileType = request.ProfileType.Value;
            }

            if (request.CompanyName != null)
            {
                profile.CompanyName = request.CompanyName;
            }

            if (request.Bio != null)
            {
                profile.Bio = request.Bio;
            }

            if (request.AvatarUrl != null)
            {
                profile.AvatarUrl = request.AvatarUrl;
            }

            var updatedProfile = await _profileRepository.UpdateAsync(profile, cancellationToken);

            return GenericResponse<ProfileResponseDto>.CreateSuccess(MapToDto(updatedProfile), "Profile updated successfully");
        }

        public async Task<GenericResponse<bool>> DeleteProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var profile = await _profileRepository.GetByIdAsync(id, cancellationToken);
            if (profile == null || profile.UserId != userId)
            {
                return GenericResponse<bool>.CreateError("Profile not found", HttpStatusCode.NotFound);
            }

            var deleted = await _profileRepository.DeleteAsync(id, cancellationToken);
            if (!deleted)
            {
                return GenericResponse<bool>.CreateError("Profile not found", HttpStatusCode.NotFound);
            }

            return GenericResponse<bool>.CreateSuccess(true, "Profile deleted successfully");
        }

        public async Task<GenericResponse<bool>> RestoreProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var profile = await _profileRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (profile == null || profile.UserId != userId)
            {
                return GenericResponse<bool>.CreateError("Profile not found", HttpStatusCode.NotFound);
            }

            await _profileRepository.RestoreAsync(id, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Profile restored successfully");
        }

        private static ProfileResponseDto MapToDto(Profile profile)
        {
            return new ProfileResponseDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                WorkspaceId = profile.WorkspaceId,
                Name = profile.Name,
                ProfileType = profile.ProfileType,
                SubscriptionId = profile.SubscriptionId,
                CompanyName = profile.CompanyName,
                Bio = profile.Bio,
                AvatarUrl = profile.AvatarUrl,
                Status = profile.Status,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt,
                IsOwner = true
            };
        }
    }
}
