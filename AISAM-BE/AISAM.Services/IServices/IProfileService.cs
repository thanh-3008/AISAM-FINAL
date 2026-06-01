using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IProfileService
    {
        Task<GenericResponse<ProfileResponseDto>> GetProfileByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<IEnumerable<ProfileResponseDto>>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProfileResponseDto>> CreateProfileAsync(Guid userId, CreateProfileRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<ProfileResponseDto>> UpdateProfileAsync(Guid id, Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> DeleteProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> RestoreProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
