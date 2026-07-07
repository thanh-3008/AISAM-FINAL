using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Response;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using AISAM.Data.Model;

namespace AISAM.Repositories.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AisamContext _context;

        public UserRepository(AisamContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByPasswordResetTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        }

        public async Task<PagedResult<UserListDto>> GetPagedUsersAsync(PaginationRequest request)
        {
            var query = _context.Users.AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(u => u.Email!.Contains(request.SearchTerm));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "createdat" => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            // Apply pagination and get user data with social accounts count
            var users = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    CreatedAt = u.CreatedAt,
                    SocialAccountsCount = _context.SocialAccounts
                        .Count(sa => _context.Profiles
                            .Any(p => p.UserId == u.Id && p.Id == sa.ProfileId && !sa.IsDeleted))
                })
                .ToListAsync();

            return new PagedResult<UserListDto>
            {
                Data = users,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users.CountAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<PagedResult<UserListDto>> GetPagedUsersWithRoleFilterAsync(
            PaginationRequest request, int? role, bool? isEmailVerified, string? search, CancellationToken cancellationToken = default)
        {
            var query = _context.Users.AsNoTracking();

            if (role.HasValue)
                query = query.Where(u => (int)u.Role == role.Value);
            if (isEmailVerified.HasValue)
                query = query.Where(u => u.IsEmailVerified == isEmailVerified.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Email.Contains(search) || (u.FullName != null && u.FullName.Contains(search)));

            var total = await query.CountAsync(cancellationToken);
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = users.Select(u => new UserListDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = (int)u.Role,
                RoleName = u.Role.ToString(),
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt
            }).ToList();

            return new PagedResult<UserListDto> { Data = dtos, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
        }
    }
}
