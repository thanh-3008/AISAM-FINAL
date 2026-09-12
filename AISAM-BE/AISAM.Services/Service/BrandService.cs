using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Access;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AISAM.Services.Service
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
        private readonly AisamContext? _context;

        public BrandService(
            IBrandRepository brandRepository,
            IProfileRepository profileRepository,
            IWorkspaceMemberRepository workspaceMemberRepository,
            AisamContext? context = null)
        {
            _brandRepository = brandRepository;
            _profileRepository = profileRepository;
            _workspaceMemberRepository = workspaceMemberRepository;
            _context = context;
        }

        public async Task<GenericResponse<PagedResult<BrandResponseDto>>> GetPagedByWorkspaceIdAsync(
            Guid workspaceId,
            Guid userId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<PagedResult<BrandResponseDto>>.CreateError(access.Message);
            }

            var brands = await _brandRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, includeDeleted, cancellationToken);

            return GenericResponse<PagedResult<BrandResponseDto>>.CreateSuccess(new PagedResult<BrandResponseDto>
            {
                Data = brands.Data.Select(MapToDto).ToList(),
                TotalCount = brands.TotalCount,
                Page = brands.Page,
                PageSize = brands.PageSize
            }, "Brands retrieved successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<BrandResponseDto>.CreateError("Brand not found");
            }

            var access = await EnsureBrandWorkspaceAccessAsync(brand, workspaceId, userId, cancellationToken, requireOwnerOrManager: true);
            if (!access.Success)
            {
                return GenericResponse<BrandResponseDto>.CreateError(access.Message);
            }

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(brand), "Brand retrieved successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> CreateAsync(Guid workspaceId, Guid userId, CreateBrandRequest request, CancellationToken cancellationToken = default)
        {
            var (success, message, membership) = await EnsureWorkspaceOwnerOrManagerAsync(workspaceId, userId, cancellationToken);
            if (!success || membership == null)
            {
                return GenericResponse<BrandResponseDto>.CreateError(message);
            }

            if (membership.Role == WorkspaceMemberRoleEnum.Manager && _context != null)
            {
                var teamIds = await (from m in _context.TeamMembers
                                     join t in _context.Teams on m.TeamId equals t.Id
                                     where m.UserId == userId && m.IsActive && t.WorkspaceId == workspaceId && !t.IsDeleted && t.Status == TeamStatusEnum.Active
                                     select m.TeamId).ToListAsync(cancellationToken);
                if (teamIds.Count == 0)
                {
                    return GenericResponse<BrandResponseDto>.CreateError("Manager needs BrandCreate permission to create brands");
                }

                var perms = await _context.TeamMembers
                    .Where(m => m.UserId == userId && m.IsActive && teamIds.Contains(m.TeamId))
                    .Select(m => m.Permissions).ToListAsync(cancellationToken);

                bool hasPerm = perms.Any(list => list != null && list.Contains(DelegatedPermissionKeys.BrandCreate, StringComparer.Ordinal));
                if (!hasPerm)
                {
                    return GenericResponse<BrandResponseDto>.CreateError("Manager needs BrandCreate permission to create brands");
                }
            }

            var profile = request.ProfileId.HasValue
                ? await _profileRepository.GetByIdAsync(request.ProfileId.Value, cancellationToken)
                : (await _profileRepository.GetByUserIdAsync(userId, cancellationToken)).FirstOrDefault();

            if (profile == null)
            {
                if (request.ProfileId.HasValue)
                {
                    return GenericResponse<BrandResponseDto>.CreateError("Profile not found");
                }

                profile = await _profileRepository.CreateAsync(new Profile
                {
                    UserId = userId,
                    Name = "Workspace Profile",
                    ProfileType = ProfileTypeEnum.Free,
                    Status = ProfileStatusEnum.Active
                }, cancellationToken);
            }

            if (profile.UserId != userId)
            {
                return GenericResponse<BrandResponseDto>.CreateError("You are not allowed to access this profile");
            }

            if (await _brandRepository.ExistsByNameInWorkspaceAsync(workspaceId, request.Name, cancellationToken))
            {
                return GenericResponse<BrandResponseDto>.CreateError("Brand name already exists in this workspace");
            }

            var brand = new Brand
            {
                ProfileId = profile.Id,
                Profile = profile,
                WorkspaceId = workspaceId,
                Name = request.Name,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                Slogan = request.Slogan,
                Usp = request.Usp,
                TargetAudience = request.TargetAudience
            };

            var created = await _brandRepository.AddAsync(brand, cancellationToken);

            if (_context != null && membership.Role == WorkspaceMemberRoleEnum.Manager)
            {
                var teamId = await (from m in _context.TeamMembers
                                    join t in _context.Teams on m.TeamId equals t.Id
                                    where m.UserId == userId && m.IsActive && t.WorkspaceId == workspaceId && !t.IsDeleted && t.Status == TeamStatusEnum.Active
                                    select t.Id).FirstOrDefaultAsync(cancellationToken);
                if (teamId != Guid.Empty)
                {
                    _context.TeamBrands.Add(new TeamBrand
                    {
                        TeamId = teamId,
                        BrandId = created.Id,
                        IsActive = true,
                        AssignedAt = DateTime.UtcNow
                    });
                    _context.AuditLogs.Add(new AuditLog
                    {
                        ActorId = userId,
                        WorkspaceId = workspaceId,
                        TeamId = teamId,
                        ActionType = "brand.auto_assign_team",
                        TargetTable = "team_brands",
                        TargetId = created.Id,
                        Result = "allowed",
                        NewValues = JsonSerializer.Serialize(new { TeamId = teamId, BrandId = created.Id })
                    });
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(created), "Brand created successfully");
        }

        public async Task<GenericResponse<BrandResponseDto>> UpdateAsync(Guid id, Guid workspaceId, Guid userId, UpdateBrandRequest request, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<BrandResponseDto>.CreateError("Brand not found");
            }

            var access = await EnsureBrandWorkspaceAccessAsync(brand, workspaceId, userId, cancellationToken, requireOwnerOrManager: true);
            if (!access.Success)
            {
                return GenericResponse<BrandResponseDto>.CreateError(access.Message);
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                if (request.Name != brand.Name &&
                    await _brandRepository.ExistsByNameInWorkspaceAsync(workspaceId, request.Name, cancellationToken))
                {
                    return GenericResponse<BrandResponseDto>.CreateError("Brand name already exists in this workspace");
                }
                brand.Name = request.Name;
            }

            if (request.Description != null)
            {
                brand.Description = request.Description;
            }

            if (request.LogoUrl != null)
            {
                brand.LogoUrl = request.LogoUrl;
            }

            if (request.Slogan != null)
            {
                brand.Slogan = request.Slogan;
            }

            if (request.Usp != null)
            {
                brand.Usp = request.Usp;
            }

            if (request.TargetAudience != null)
            {
                brand.TargetAudience = request.TargetAudience;
            }

            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<BrandResponseDto>.CreateSuccess(MapToDto(brand), "Brand updated successfully");
        }

        public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<bool>.CreateError("Brand not found");
            }

            var access = await EnsureBrandWorkspaceAccessAsync(brand, workspaceId, userId, cancellationToken, requireOwnerOrManager: true);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            if (brand.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Brand is already deleted");
            }

            var activeProducts = brand.Products?.Count(p => !p.IsDeleted) ?? 0;
            if (activeProducts > 0)
            {
                return GenericResponse<bool>.CreateError("Cannot delete brand with existing products. Please delete or reassign products first.");
            }

            brand.IsDeleted = true;
            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Brand deleted successfully");
        }

        public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var brand = await _brandRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (brand == null)
            {
                return GenericResponse<bool>.CreateError("Brand not found");
            }

            var access = await EnsureBrandWorkspaceAccessAsync(brand, workspaceId, userId, cancellationToken, requireOwnerOrManager: true);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            if (!brand.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Brand is not deleted");
            }

            brand.IsDeleted = false;
            await _brandRepository.UpdateAsync(brand, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Brand restored successfully");
        }

        private async Task<(bool Success, string Message)> EnsureProfileOwnerAsync(Guid profileId, Guid userId, CancellationToken cancellationToken)
        {
            var profile = await _profileRepository.GetByIdAsync(profileId, cancellationToken);
            if (profile == null)
            {
                return (false, "Profile not found");
            }

            if (profile.UserId != userId)
            {
                return (false, "You are not allowed to access this profile");
            }

            return (true, string.Empty);
        }

        private async Task<(bool Success, string Message)> EnsureWorkspaceMemberAsync(
            Guid workspaceId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var membership = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
            return membership == null
                ? (false, "You are not allowed to access this workspace")
                : (true, string.Empty);
        }

        private async Task<(bool Success, string Message, WorkspaceMember? Membership)> EnsureWorkspaceOwnerOrManagerAsync(
            Guid workspaceId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var membership = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
            if (membership == null)
                return (false, "You are not allowed to access this workspace", null);

            if (membership.Role != WorkspaceMemberRoleEnum.Owner && membership.Role != WorkspaceMemberRoleEnum.Manager)
                return (false, "Only workspace Owner and Manager can manage brands", null);

            return (true, string.Empty, membership);
        }

        private async Task<(bool Success, string Message)> EnsureBrandWorkspaceAccessAsync(
            Brand brand,
            Guid workspaceId,
            Guid userId,
            CancellationToken cancellationToken,
            bool requireOwnerOrManager = false)
        {
            if (brand.WorkspaceId != workspaceId)
            {
                return (false, "Brand not found");
            }

            if (requireOwnerOrManager)
            {
                var (s, m, _) = await EnsureWorkspaceOwnerOrManagerAsync(workspaceId, userId, cancellationToken);
                return (s, m);
            }

            return await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
        }

        private static BrandResponseDto MapToDto(Brand brand)
        {
            return new BrandResponseDto
            {
                Id = brand.Id,
                UserId = brand.Profile?.UserId ?? Guid.Empty,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Slogan = brand.Slogan,
                Usp = brand.Usp,
                TargetAudience = brand.TargetAudience,
                ProfileId = brand.ProfileId,
                WorkspaceId = brand.WorkspaceId,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt,
                IsDeleted = brand.IsDeleted,
                ProductsCount = brand.Products?.Count(p => !p.IsDeleted) ?? 0,
                ContentsCount = brand.Contents?.Count(c => !c.IsDeleted) ?? 0
            };
        }
    }
}
