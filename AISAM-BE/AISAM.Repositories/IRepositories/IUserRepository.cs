using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<User?> GetByPasswordResetTokenAsync(string token);
        Task<User?> GetByEmailVerificationTokenAsync(string token);
        Task<PagedResult<UserListDto>> GetPagedUsersAsync(PaginationRequest request);
        Task<int> GetCountAsync(CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<UserListDto>> GetPagedUsersWithRoleFilterAsync(PaginationRequest request, int? role, bool? isEmailVerified, string? search, CancellationToken cancellationToken = default);
        Task<Dictionary<DateTime, int>> GetDailyRegistrationsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetAdminsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Session>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
