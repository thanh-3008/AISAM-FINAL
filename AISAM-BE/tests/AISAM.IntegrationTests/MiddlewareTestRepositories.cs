using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;

namespace AISAM.IntegrationTests;

internal sealed class EmptyWorkspaceRepository : IWorkspaceRepository
{
    public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Workspace?>(null);
    public Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Workspace?>(null);
    public Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Workspace>>([]);
    public Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default) => Task.FromResult(workspace);
    public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<PagedResult<Workspace>> GetPagedAllAsync(PaginationRequest request, int? workspaceType = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Workspace>> GetAllActiveAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Workspace>>([]);
}

internal sealed class EmptyUserRepository : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id) => Task.FromResult<User?>(null);
    public Task<User?> GetByEmailAsync(string email) => Task.FromResult<User?>(null);
    public Task<User> CreateAsync(User user) => Task.FromResult(user);
    public Task<User> UpdateAsync(User user) => Task.FromResult(user);
    public Task<User?> GetByPasswordResetTokenAsync(string token) => Task.FromResult<User?>(null);
    public Task<User?> GetByEmailVerificationTokenAsync(string token) => Task.FromResult<User?>(null);
    public Task<PagedResult<UserListDto>> GetPagedUsersAsync(PaginationRequest request) => Task.FromResult(new PagedResult<UserListDto>());
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<PagedResult<UserListDto>> GetPagedUsersWithRoleFilterAsync(PaginationRequest request, int? role, bool? isEmailVerified, string? search, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Dictionary<DateTime, int>> GetDailyRegistrationsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
    public Task<IReadOnlyList<User>> GetAdminsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<User>>(Array.Empty<User>());
    public Task<IReadOnlyList<Session>> GetSessionsAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Session>>(Array.Empty<Session>());
    public Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<User>>(Array.Empty<User>());
}
