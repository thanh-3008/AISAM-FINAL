# Campaigns Feature Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement full Campaigns CRUD API on the backend and connect the frontend from localStorage mock data to real API calls.

**Architecture:** Follow the existing Controller-Service-Repository pattern (mirroring Brand feature). Backend exposes REST API at `/api/campaigns` with workspace-scoped access control. Frontend replaces `campaignService.ts` localStorage mock with `apiClient` calls.

**Tech Stack:** C# .NET 8 / ASP.NET Core / EF Core / PostgreSQL (BE), Next.js 16 / TypeScript / Tailwind CSS v4 (FE)

---

## Current State

| Layer | Status |
|-------|--------|
| BE Data Models | AdCampaign, AdSet, Ad, AdCreative, PerformanceReport exist + migrated |
| BE Repository | **Missing** - no IAdCampaignRepository / AdCampaignRepository |
| BE Service | **Missing** - no IAdCampaignService / AdCampaignService |
| BE Controller | **Missing** - no AdCampaignController |
| BE Middleware | Missing `/api/campaigns` in ProtectedPrefixes + route permission check |
| BE Permission | Missing `ManageCampaigns` in WorkspacePermissionEnum |
| FE Service | campaignService.ts uses localStorage mock (100% mock) |
| FE Dashboard | `campaignsData` array hardcoded (5 items) with CSV export |
| FE Brand Detail | Campaigns tab always shows empty state |
| FE Analytics | `campaignPerformance` always returns `[]` |

---

## File Structure Map

```
BE (AISAM-BE):
  Create: AISAM.Common/Dtos/Request/CreateAdCampaignRequest.cs
  Create: AISAM.Common/Dtos/Request/UpdateAdCampaignRequest.cs
  Create: AISAM.Common/Dtos/Response/AdCampaignResponseDto.cs
  Create: AISAM.Repositories/IRepositories/IAdCampaignRepository.cs
  Create: AISAM.Repositories/Repository/AdCampaignRepository.cs
  Create: AISAM.Services/IServices/IAdCampaignService.cs
  Create: AISAM.Services/Service/AdCampaignService.cs
  Create: AISAM.API/Controllers/AdCampaignController.cs
  Modify: AISAM.API/Program.cs (add DI registrations)
  Modify: AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs (add route protection)
  Modify: AISAM.Data/Enumeration/WorkspacePermissionEnum.cs (add ManageCampaigns)

FE (AISAM-FE):
  Modify: src/services/campaignService.ts (replace localStorage mock with API calls)
  Modify: src/components/campaigns/campaignUtils.ts (remove hardcoded BRANDS)
  Modify: src/components/campaigns/CreateCampaignModal.tsx (fetch brands from API)
  Modify: src/components/campaigns/EditCampaignModal.tsx (fetch brands from API)
  Modify: src/app/(dashboard)/dashboard/page.tsx (replace campaignsData hardcode with API)
  Modify: src/app/(dashboard)/brands/[id]/page.tsx (connect campaigns tab to API)
  Modify: src/app/(dashboard)/analytics/page.tsx (connect campaign performance to API)
  Modify: src/services/analyticsService.ts (campaignPerformance from BE)
```

---

### Task 1: Add ManageCampaigns permission to WorkspacePermissionEnum

**Files:**
- Modify: `AISAM-BE\AISAM.Data\Enumeration\WorkspacePermissionEnum.cs`

- [ ] **Step 1: Add the enum value**

```csharp
namespace AISAM.Data.Enumeration
{
    public enum WorkspacePermissionEnum
    {
        ManageBilling = 1,
        ManageBrands = 2,
        ManageProducts = 3,
        ManageContent = 4,
        PublishContent = 5,
        GenerateAiContent = 6,
        ManageSchedules = 7,
        ManageCampaigns = 8
    }
}
```

- [ ] **Step 2: Build to verify**

```powershell
dotnet build AISAM-BE/AISAM.sln
```
Expected: Build succeeds with no errors.

- [ ] **Step 3: Commit**

```bash
git add AISAM-BE/AISAM.Data/Enumeration/WorkspacePermissionEnum.cs
git commit -m "feat: add ManageCampaigns permission enum"
```

---

### Task 2: Create Campaign DTOs (Request + Response)

**Files:**
- Create: `AISAM-BE\AISAM.Common\Dtos\Request\CreateAdCampaignRequest.cs`
- Create: `AISAM-BE\AISAM.Common\Dtos\Request\UpdateAdCampaignRequest.cs`
- Create: `AISAM-BE\AISAM.Common\Dtos\Response\AdCampaignResponseDto.cs`

- [ ] **Step 1: Create CreateAdCampaignRequest.cs**

```csharp
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class CreateAdCampaignRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid BrandId { get; set; }

        [Required]
        [MaxLength(255)]
        public string AdAccountId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Objective { get; set; }

        public decimal? Budget { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
```

- [ ] **Step 2: Create UpdateAdCampaignRequest.cs**

```csharp
using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class UpdateAdCampaignRequest
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        public Guid? BrandId { get; set; }

        [MaxLength(255)]
        public string? AdAccountId { get; set; }

        [MaxLength(100)]
        public string? Objective { get; set; }

        public decimal? Budget { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
```

- [ ] **Step 3: Create AdCampaignResponseDto.cs**

```csharp
namespace AISAM.Common.Dtos.Response
{
    public class AdCampaignResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public Guid WorkspaceId { get; set; }
        public Guid BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string AdAccountId { get; set; } = string.Empty;
        public string? FacebookCampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Objective { get; set; }
        public decimal? Budget { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<AdSetSummaryDto> AdSets { get; set; } = new();
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public decimal Spend { get; set; }
        public long Conversions { get; set; }
    }

    public class AdSetSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? FacebookAdSetId { get; set; }
        public decimal? DailyBudget { get; set; }
        public string? Status { get; set; }
        public long Impressions { get; set; }
        public long Clicks { get; set; }
        public decimal Spend { get; set; }
    }
}
```

- [ ] **Step 4: Build to verify**

```powershell
dotnet build AISAM-BE/AISAM.sln
```
Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add AISAM-BE/AISAM.Common/Dtos/Request/CreateAdCampaignRequest.cs AISAM-BE/AISAM.Common/Dtos/Request/UpdateAdCampaignRequest.cs AISAM-BE/AISAM.Common/Dtos/Response/AdCampaignResponseDto.cs
git commit -m "feat: add campaign DTOs (request + response)"
```

---

### Task 3: Create IAdCampaignRepository and AdCampaignRepository

**Files:**
- Create: `AISAM-BE\AISAM.Repositories\IRepositories\IAdCampaignRepository.cs`
- Create: `AISAM-BE\AISAM.Repositories\Repository\AdCampaignRepository.cs`

- [ ] **Step 1: Create IAdCampaignRepository.cs**

```csharp
using AISAM.Common.Dtos;
using AISAM.Data.Model;

namespace AISAM.Repositories.IRepositories
{
    public interface IAdCampaignRepository
    {
        Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AdCampaign?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<AdCampaign>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<AdCampaign> AddAsync(AdCampaign campaign, CancellationToken cancellationToken = default);
        Task UpdateAsync(AdCampaign campaign, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 2: Create AdCampaignRepository.cs**

```csharp
using AISAM.Common.Dtos;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.Repositories.Repository
{
    public class AdCampaignRepository : IAdCampaignRepository
    {
        private readonly AisamContext _context;

        public AdCampaignRepository(AisamContext context)
        {
            _context = context;
        }

        public async Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AdCampaigns
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.AdSets.Where(ads => !ads.IsDeleted))
                .FirstOrDefaultAsync(ac => ac.Id == id && !ac.IsDeleted, cancellationToken);
        }

        public async Task<AdCampaign?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AdCampaigns
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.AdSets)
                .FirstOrDefaultAsync(ac => ac.Id == id, cancellationToken);
        }

        public async Task<PagedResult<AdCampaign>> GetPagedByWorkspaceIdAsync(
            Guid workspaceId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var query = _context.AdCampaigns
                .AsSplitQuery()
                .Include(ac => ac.Brand)
                .Include(ac => ac.AdSets.Where(ads => !ads.IsDeleted))
                .Where(ac => ac.WorkspaceId == workspaceId);

            if (!includeDeleted)
            {
                query = query.Where(ac => !ac.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchPattern = $"%{request.SearchTerm}%";
                query = query.Where(ac =>
                    EF.Functions.ILike(ac.Name, searchPattern) ||
                    (ac.Objective != null && EF.Functions.ILike(ac.Objective, searchPattern)));
            }

            query = (request.SortBy ?? string.Empty).ToLowerInvariant() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(ac => ac.Name) : query.OrderBy(ac => ac.Name),
                "budget" => request.SortDescending ? query.OrderByDescending(ac => ac.Budget) : query.OrderBy(ac => ac.Budget),
                "startdate" => request.SortDescending ? query.OrderByDescending(ac => ac.StartDate) : query.OrderBy(ac => ac.StartDate),
                _ => query.OrderByDescending(ac => ac.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return new PagedResult<AdCampaign>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AdCampaign> AddAsync(AdCampaign campaign, CancellationToken cancellationToken = default)
        {
            campaign.CreatedAt = DateTime.UtcNow;
            campaign.UpdatedAt = DateTime.UtcNow;

            _context.AdCampaigns.Add(campaign);
            await _context.SaveChangesAsync(cancellationToken);
            return campaign;
        }

        public async Task UpdateAsync(AdCampaign campaign, CancellationToken cancellationToken = default)
        {
            campaign.UpdatedAt = DateTime.UtcNow;
            _context.AdCampaigns.Update(campaign);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

- [ ] **Step 3: Build to verify**

```powershell
dotnet build AISAM-BE/AISAM.sln
```
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add AISAM-BE/AISAM.Repositories/IRepositories/IAdCampaignRepository.cs AISAM-BE/AISAM.Repositories/Repository/AdCampaignRepository.cs
git commit -m "feat: add AdCampaign repository"
```

---

### Task 4: Create IAdCampaignService and AdCampaignService

**Files:**
- Create: `AISAM-BE\AISAM.Services\IServices\IAdCampaignService.cs`
- Create: `AISAM-BE\AISAM.Services\Service\AdCampaignService.cs`

- [ ] **Step 1: Create IAdCampaignService.cs**

```csharp
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;

namespace AISAM.Services.IServices
{
    public interface IAdCampaignService
    {
        Task<GenericResponse<PagedResult<AdCampaignResponseDto>>> GetPagedByWorkspaceIdAsync(Guid workspaceId, Guid userId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> CreateAsync(Guid workspaceId, Guid userId, CreateAdCampaignRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<AdCampaignResponseDto>> UpdateAsync(Guid id, Guid workspaceId, Guid userId, UpdateAdCampaignRequest request, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
        Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
    }
}
```

- [ ] **Step 2: Create AdCampaignService.cs**

```csharp
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;

namespace AISAM.Services.Service
{
    public class AdCampaignService : IAdCampaignService
    {
        private readonly IAdCampaignRepository _campaignRepository;
        private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
        private readonly IBrandRepository _brandRepository;

        public AdCampaignService(
            IAdCampaignRepository campaignRepository,
            IWorkspaceMemberRepository workspaceMemberRepository,
            IBrandRepository brandRepository)
        {
            _campaignRepository = campaignRepository;
            _workspaceMemberRepository = workspaceMemberRepository;
            _brandRepository = brandRepository;
        }

        public async Task<GenericResponse<PagedResult<AdCampaignResponseDto>>> GetPagedByWorkspaceIdAsync(
            Guid workspaceId,
            Guid userId,
            PaginationRequest request,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateError(access.Message);
            }

            var campaigns = await _campaignRepository.GetPagedByWorkspaceIdAsync(workspaceId, request, includeDeleted, cancellationToken);

            return GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateSuccess(new PagedResult<AdCampaignResponseDto>
            {
                Data = campaigns.Data.Select(MapToDto).ToList(),
                TotalCount = campaigns.TotalCount,
                Page = campaigns.Page,
                PageSize = campaigns.PageSize
            }, "Campaigns retrieved successfully");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> GetByIdAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");
            }

            if (campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);
            }

            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(campaign), "Campaign retrieved successfully");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> CreateAsync(Guid workspaceId, Guid userId, CreateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);
            }

            var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);
            if (brand == null || brand.WorkspaceId != workspaceId)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Brand not found in this workspace");
            }

            var campaign = new AdCampaign
            {
                WorkspaceId = workspaceId,
                ProfileId = brand.ProfileId,
                BrandId = request.BrandId,
                AdAccountId = request.AdAccountId,
                Name = request.Name,
                Objective = request.Objective,
                Budget = request.Budget,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            var created = await _campaignRepository.AddAsync(campaign, cancellationToken);

            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(created), "Campaign created successfully");
        }

        public async Task<GenericResponse<AdCampaignResponseDto>> UpdateAsync(Guid id, Guid workspaceId, Guid userId, UpdateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<AdCampaignResponseDto>.CreateError(access.Message);
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                campaign.Name = request.Name;
            }

            if (request.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByIdAsync(request.BrandId.Value, cancellationToken);
                if (brand == null || brand.WorkspaceId != workspaceId)
                {
                    return GenericResponse<AdCampaignResponseDto>.CreateError("Brand not found in this workspace");
                }

                campaign.BrandId = request.BrandId.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.AdAccountId))
            {
                campaign.AdAccountId = request.AdAccountId;
            }

            if (request.Objective != null)
            {
                campaign.Objective = request.Objective;
            }

            if (request.Budget.HasValue)
            {
                campaign.Budget = request.Budget.Value;
            }

            if (request.StartDate.HasValue)
            {
                campaign.StartDate = request.StartDate.Value;
            }

            if (request.EndDate.HasValue)
            {
                campaign.EndDate = request.EndDate.Value;
            }

            if (request.IsActive.HasValue)
            {
                campaign.IsActive = request.IsActive.Value;
            }

            await _campaignRepository.UpdateAsync(campaign, cancellationToken);

            return GenericResponse<AdCampaignResponseDto>.CreateSuccess(MapToDto(campaign), "Campaign updated successfully");
        }

        public async Task<GenericResponse<bool>> SoftDeleteAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<bool>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            campaign.IsDeleted = true;
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Campaign deleted successfully");
        }

        public async Task<GenericResponse<bool>> RestoreAsync(Guid id, Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdIncludingDeletedAsync(id, cancellationToken);
            if (campaign == null || campaign.WorkspaceId != workspaceId)
            {
                return GenericResponse<bool>.CreateError("Campaign not found");
            }

            var access = await EnsureWorkspaceMemberAsync(workspaceId, userId, cancellationToken);
            if (!access.Success)
            {
                return GenericResponse<bool>.CreateError(access.Message);
            }

            if (!campaign.IsDeleted)
            {
                return GenericResponse<bool>.CreateError("Campaign is not deleted");
            }

            campaign.IsDeleted = false;
            await _campaignRepository.UpdateAsync(campaign, cancellationToken);

            return GenericResponse<bool>.CreateSuccess(true, "Campaign restored successfully");
        }

        private async Task<(bool Success, string Message)> EnsureWorkspaceMemberAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken)
        {
            var membership = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);
            return membership == null
                ? (false, "You are not allowed to access this workspace")
                : (true, string.Empty);
        }

        private static AdCampaignResponseDto MapToDto(AdCampaign campaign)
        {
            return new AdCampaignResponseDto
            {
                Id = campaign.Id,
                ProfileId = campaign.ProfileId,
                WorkspaceId = campaign.WorkspaceId,
                BrandId = campaign.BrandId,
                BrandName = campaign.Brand?.Name ?? string.Empty,
                AdAccountId = campaign.AdAccountId,
                FacebookCampaignId = campaign.FacebookCampaignId,
                Name = campaign.Name,
                Objective = campaign.Objective,
                Budget = campaign.Budget,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                IsActive = campaign.IsActive,
                IsDeleted = campaign.IsDeleted,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                AdSets = campaign.AdSets.Select(ads => new AdSetSummaryDto
                {
                    Id = ads.Id,
                    Name = ads.Name,
                    FacebookAdSetId = ads.FacebookAdSetId,
                    DailyBudget = ads.DailyBudget,
                    Status = ads.Status,
                    Impressions = 0,
                    Clicks = 0,
                    Spend = 0
                }).ToList(),
                Impressions = 0,
                Clicks = 0,
                Spend = 0,
                Conversions = 0
            };
        }
    }
}
```

- [ ] **Step 3: Build to verify**

```powershell
dotnet build AISAM-BE/AISAM.sln
```
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add AISAM-BE/AISAM.Services/IServices/IAdCampaignService.cs AISAM-BE/AISAM.Services/Service/AdCampaignService.cs
git commit -m "feat: add AdCampaign service"
```

---

### Task 5: Create AdCampaignController

**Files:**
- Create: `AISAM-BE\AISAM.API\Controllers\AdCampaignController.cs`

- [ ] **Step 1: Create AdCampaignController.cs**

```csharp
using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace AISAM.API.Controllers
{
    [ApiController]
    [Route("api/campaigns")]
    [Authorize]
    public class AdCampaignController : ControllerBase
    {
        private readonly IAdCampaignService _campaignService;
        private readonly ILogger<AdCampaignController> _logger;

        public AdCampaignController(IAdCampaignService campaignService, ILogger<AdCampaignController> logger)
        {
            _campaignService = campaignService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<GenericResponse<PagedResult<AdCampaignResponseDto>>>> GetCampaigns(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = true,
            [FromQuery] bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.GetPagedByWorkspaceIdAsync(workspaceId, userId, new PaginationRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                }, includeDeleted, cancellationToken);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaigns");
                return StatusCode(500, GenericResponse<PagedResult<AdCampaignResponseDto>>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.GetByIdAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> Create([FromBody] CreateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.CreateAsync(workspaceId, userId, request, cancellationToken);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign");
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GenericResponse<AdCampaignResponseDto>>> Update(Guid id, [FromBody] UpdateAdCampaignRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.UpdateAsync(id, workspaceId, userId, request, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<AdCampaignResponseDto>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<AdCampaignResponseDto>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<GenericResponse<bool>>> SoftDelete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.SoftDeleteAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult<GenericResponse<bool>>> Restore(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetUserIdOrThrow();
                var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
                var result = await _campaignService.RestoreAsync(id, workspaceId, userId, cancellationToken);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(GenericResponse<bool>.CreateError("Invalid token"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring campaign {CampaignId}", id);
                return StatusCode(500, GenericResponse<bool>.CreateError("System error", HttpStatusCode.InternalServerError));
            }
        }

        private Guid GetUserIdOrThrow()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            return userId;
        }
    }
}
```

- [ ] **Step 2: Build to verify**

```powershell
dotnet build AISAM-BE/AISAM.sln
```
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add AISAM-BE/AISAM.API/Controllers/AdCampaignController.cs
git commit -m "feat: add AdCampaign controller"
```

---

### Task 6: Register DI and Middleware

**Files:**
- Modify: `AISAM-BE\AISAM.API\Program.cs` (add 2 lines for DI)
- Modify: `AISAM-BE\AISAM.API\Middleware\ActiveWorkspaceMiddleware.cs` (add route protection)

- [ ] **Step 1: Add DI registrations in Program.cs**

Add after line 131 (after `IPerformanceReportRepository`):
```csharp
builder.Services.AddScoped<IAdCampaignRepository, AdCampaignRepository>();
```

Add after line 163 (after `IDashboardService`):
```csharp
builder.Services.AddScoped<IAdCampaignService, AdCampaignService>();
```

- [ ] **Step 2: Add ProtectedPrefix in ActiveWorkspaceMiddleware.cs**

Add after line 32 `new("/api/credit-usage")`:
```csharp
        new("/api/campaigns"),
```

- [ ] **Step 3: Add route permission check block in ActiveWorkspaceMiddleware.cs**

Add after the brands permission block (after line 149 `}`), before the products block:

```csharp
        if (path.StartsWithSegments("/api/campaigns"))
        {
            if (method == HttpMethods.Get)
            {
                return null;
            }

            return EnsurePermission(membership.Role, WorkspacePermissionEnum.ManageCampaigns);
        }
```

- [ ] **Step 4: Build to verify**

```powershell
dotnet build AISAM-BE/AISAM.sln
```
Expected: Build succeeds.

- [ ] **Step 5: Run the BE to verify startup**

```powershell
dotnet run --project AISAM-BE/AISAM.API
```
Expected: App starts without errors. Then Ctrl+C to stop.

- [ ] **Step 6: Commit**

```bash
git add AISAM-BE/AISAM.API/Program.cs AISAM-BE/AISAM.API/Middleware/ActiveWorkspaceMiddleware.cs
git commit -m "feat: register campaign DI and add middleware protection"
```

---

### Task 7: Rewrite FE campaignService.ts to use real API

**Files:**
- Modify: `AISAM-FE\src\services\campaignService.ts`

- [ ] **Step 1: Replace entire campaignService.ts**

```typescript
import { apiClient } from "@/lib/apiClient";

export type CampaignStatus = "ACTIVE" | "PAUSED" | "COMPLETED" | "DRAFT";
export type CampaignObjective = "AWARENESS" | "TRAFFIC" | "ENGAGEMENT" | "LEADS" | "SALES" | "APP_PROMOTION";

export interface AdSet {
  id: string;
  name: string;
  facebookAdSetId: string | null;
  dailyBudget: number | null;
  status: "ACTIVE" | "PAUSED";
  impressions: number;
  clicks: number;
  spend: number;
}

export interface Campaign {
  id: string;
  profileId: string;
  workspaceId: string;
  brandId: string;
  brandName: string;
  adAccountId: string;
  facebookCampaignId: string | null;
  name: string;
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
  status: CampaignStatus;
  createdAt: string;
  updatedAt: string;
  adSets: AdSet[];
  impressions: number;
  clicks: number;
  spend: number;
  conversions: number;
}

export interface CreateCampaignData {
  name: string;
  brandId: string;
  brandName: string;
  objective: CampaignObjective;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
}

interface CampaignApiItem {
  id: string;
  profileId: string;
  workspaceId: string;
  brandId: string;
  brandName: string;
  adAccountId: string;
  facebookCampaignId: string | null;
  name: string;
  objective: string | null;
  budget: number | null;
  startDate: string | null;
  endDate: string | null;
  isActive: boolean;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
  adSets: AdSetApiItem[];
  impressions: number;
  clicks: number;
  spend: number;
  conversions: number;
}

interface AdSetApiItem {
  id: string;
  name: string;
  facebookAdSetId: string | null;
  dailyBudget: number | null;
  status: string | null;
  impressions: number;
  clicks: number;
  spend: number;
}

function mapCampaign(api: CampaignApiItem): Campaign {
  let status: CampaignStatus = "DRAFT";
  if (api.isActive) {
    status = "ACTIVE";
  } else if (api.endDate && new Date(api.endDate) < new Date()) {
    status = "COMPLETED";
  } else if (!api.isActive && api.startDate) {
    status = "PAUSED";
  }

  return {
    id: api.id,
    profileId: api.profileId,
    workspaceId: api.workspaceId,
    brandId: api.brandId,
    brandName: api.brandName,
    adAccountId: api.adAccountId,
    facebookCampaignId: api.facebookCampaignId,
    name: api.name,
    objective: (api.objective as CampaignObjective) || "AWARENESS",
    budget: api.budget,
    startDate: api.startDate,
    endDate: api.endDate,
    status,
    createdAt: api.createdAt,
    updatedAt: api.updatedAt,
    adSets: (api.adSets || []).map((ads) => ({
      id: ads.id,
      name: ads.name,
      facebookAdSetId: ads.facebookAdSetId,
      dailyBudget: ads.dailyBudget,
      status: (ads.status === "ACTIVE" ? "ACTIVE" : "PAUSED") as "ACTIVE" | "PAUSED",
      impressions: ads.impressions,
      clicks: ads.clicks,
      spend: ads.spend,
    })),
    impressions: api.impressions,
    clicks: api.clicks,
    spend: api.spend,
    conversions: api.conversions,
  };
}

export async function fetchCampaigns(params?: {
  page?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}): Promise<{ data: Campaign[]; total: number }> {
  try {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", String(params.page));
    if (params?.pageSize) query.set("pageSize", String(params.pageSize));
    if (params?.searchTerm) query.set("searchTerm", params.searchTerm);
    if (params?.sortBy) query.set("sortBy", params.sortBy);
    if (params?.sortDescending !== undefined) query.set("sortDescending", String(params.sortDescending));

    const qs = query.toString();
    const res = await apiClient(`/campaigns${qs ? `?${qs}` : ""}`);
    if (res?.success && res.data) {
      return {
        data: (res.data.data as CampaignApiItem[]).map(mapCampaign),
        total: res.data.totalCount || 0,
      };
    }
  } catch {
    // fallback to empty
  }
  return { data: [], total: 0 };
}

export async function createCampaign(data: CreateCampaignData): Promise<Campaign> {
  const res = await apiClient("/campaigns", {
    data: {
      name: data.name,
      brandId: data.brandId,
      adAccountId: `act_${Date.now()}`,
      objective: data.objective,
      budget: data.budget,
      startDate: data.startDate || null,
      endDate: data.endDate || null,
    },
  });

  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to create campaign");
  }

  return mapCampaign(res.data as CampaignApiItem);
}

export async function updateCampaign(id: string, data: CreateCampaignData): Promise<Campaign> {
  const res = await apiClient(`/campaigns/${id}`, {
    method: "PUT",
    data: {
      name: data.name,
      brandId: data.brandId,
      objective: data.objective,
      budget: data.budget,
      startDate: data.startDate || null,
      endDate: data.endDate || null,
    },
  });

  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to update campaign");
  }

  return mapCampaign(res.data as CampaignApiItem);
}

export async function updateCampaignStatus(id: string, status: CampaignStatus): Promise<Campaign> {
  const isActive = status === "ACTIVE";
  const res = await apiClient(`/campaigns/${id}`, {
    method: "PUT",
    data: { isActive },
  });

  if (!res?.success || !res.data) {
    throw new Error(res?.message || "Failed to update campaign status");
  }

  return mapCampaign(res.data as CampaignApiItem);
}

export async function applyCampaign(id: string): Promise<Campaign> {
  return updateCampaignStatus(id, "ACTIVE");
}

export async function restartCampaign(id: string): Promise<Campaign> {
  return updateCampaignStatus(id, "ACTIVE");
}

export async function deleteCampaign(id: string): Promise<boolean> {
  const res = await apiClient(`/campaigns/${id}`, { method: "DELETE" });
  return res?.success === true;
}

export async function getCampaignById(id: string): Promise<Campaign | null> {
  try {
    const res = await apiClient(`/campaigns/${id}`);
    if (res?.success && res.data) {
      return mapCampaign(res.data as CampaignApiItem);
    }
  } catch {
    // ignore
  }
  return null;
}
```

- [ ] **Step 2: Verify FE builds**

```powershell
npm run build --prefix AISAM-FE
```
Expected: Build succeeds (may have TS errors in other files we'll fix next).

- [ ] **Step 3: Commit**

```bash
git add AISAM-FE/src/services/campaignService.ts
git commit -m "feat: replace campaignService localStorage mock with real API calls"
```

---

### Task 8: Update campaignUtils.ts to remove hardcoded brands, fetch from API

**Files:**
- Modify: `AISAM-FE\src\components\campaigns\campaignUtils.ts`

- [ ] **Step 1: Remove hardcoded BRANDS, add factory function**

In `AISAM-FE\src\components\campaigns\campaignUtils.ts`, remove the `BRANDS` constant (lines 19-25):

```typescript
export const BRANDS = [
  { id: "b1", name: "Lumina Tech" },
  { id: "b2", name: "Summit Outdoor" },
  { id: "b3", name: "Heritage Motors" },
  { id: "b4", name: "GreenLeaf Organics" },
  { id: "b5", name: "Pulse Finance" },
];
```

Replace with:
```typescript
export interface BrandOption {
  id: string;
  name: string;
}

export let cachedBrands: BrandOption[] = [];

export function setCachedBrands(brands: BrandOption[]) {
  cachedBrands = brands;
}

export function getCachedBrands(): BrandOption[] {
  return cachedBrands;
}
```

- [ ] **Step 2: Update imports in CreateCampaignModal.tsx and EditCampaignModal.tsx**

Both files import `BRANDS` and `OBJECTIVE_CONFIG` from `./campaignUtils`. Change each import from:
```typescript
import { OBJECTIVE_CONFIG, BRANDS } from "./campaignUtils";
```
To:
```typescript
import { OBJECTIVE_CONFIG, getCachedBrands } from "./campaignUtils";
```

Then replace all `BRANDS.map(...)` with `getCachedBrands().map(...)` and `BRANDS.find(...)` with `getCachedBrands().find(...)`.

In CreateCampaignModal.tsx (line ~134): Change `const brand = BRANDS.find(...)` to `const brand = getCachedBrands().find(...)`.
In EditCampaignModal.tsx (line ~27): Change `const brand = BRANDS.find(...)` to `const brand = getCachedBrands().find(...)`.

- [ ] **Step 3: Update campaigns page to fetch and cache brands on load**

In `AISAM-FE\src\app\(dashboard)\campaigns\page.tsx`, add a brand fetch effect:

Add import:
```typescript
import { fetchBrands } from "@/services/brandService";
import { setCachedBrands } from "@/components/campaigns/campaignUtils";
```

Add inside the component, after the campaigns load useEffect:
```typescript
useEffect(() => {
  fetchBrands().then((brands) => {
    setCachedBrands(brands.map(b => ({ id: b.id, name: b.name })));
  });
}, [activeWorkspace?.id]);
```

- [ ] **Step 4: Build FE to verify**

```powershell
npm run build --prefix AISAM-FE
```
Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
git add AISAM-FE/src/components/campaigns/campaignUtils.ts AISAM-FE/src/components/campaigns/CreateCampaignModal.tsx AISAM-FE/src/components/campaigns/EditCampaignModal.tsx AISAM-FE/src/app/\(dashboard\)/campaigns/page.tsx
git commit -m "feat: connect campaign brand selector to real API brands"
```

---

### Task 9: Update dashboard page to fetch campaigns from API

**Files:**
- Modify: `AISAM-FE\src\app\(dashboard)\dashboard\page.tsx`

- [ ] **Step 1: Remove hardcoded campaignsData, add API fetch**

Remove lines 81-87 (the `campaignsData` array).

Add import at the top:
```typescript
import { fetchCampaigns, type Campaign } from "@/services/campaignService";
```

Add state inside the component:
```typescript
const [dashboardCampaigns, setDashboardCampaigns] = useState<Campaign[]>([]);
```

Add fetch effect:
```typescript
useEffect(() => {
  fetchCampaigns({ pageSize: 5 }).then((res) => {
    if (res) setDashboardCampaigns(res.data.slice(0, 5));
  });
}, [activeWorkspace?.id]);
```

- [ ] **Step 2: Replace campaignsData references**

Replace `{campaignsData.length} active` with:
```typescript
{dashboardCampaigns.length} campaigns
```

Replace CSV export button `onClick` (lines 458-465) to use `dashboardCampaigns`:
```typescript
onClick={() => {
  const csv = ["Name,Objective,Budget,Spend,Status"];
  dashboardCampaigns.forEach((c) => csv.push(`"${c.name}","${c.objective}","$${c.budget || 0}","$${c.spend}","${c.status}"`));
  const blob = new Blob([csv.join("\n")], { type: "text/csv" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a"); a.href = url; a.download = "campaigns-export.csv"; a.click();
  URL.revokeObjectURL(url);
}}
```

Replace the entire `campaignsData.map(...)` table body (lines 482-515) with:
```typescript
{dashboardCampaigns.map((row, i) => (
  <tr
    key={row.id}
    className="group hover:bg-surface-container/40 transition-colors duration-150"
    style={{ animation: `slide-up-row 0.4s ease-out ${0.5 + i * 0.08}s forwards`, opacity: 0 }}
  >
    <td className="px-6 py-4">
      <div className="flex items-center gap-3">
        <div className="w-8 h-8 rounded-lg bg-surface-container-high flex items-center justify-center group-hover:scale-110 group-hover:bg-primary/10 transition-all duration-300">
          <span className="material-symbols-outlined text-outline group-hover:text-primary text-[16px] transition-colors">campaign</span>
        </div>
        <span className="text-body-sm font-medium text-on-surface group-hover:text-primary transition-colors">{row.name}</span>
      </div>
    </td>
    <td className="px-6 py-4">
      <span className={`px-2.5 py-1 rounded-lg text-label-xs font-bold tracking-wide inline-block hover:scale-105 transition-transform ${row.objective === "SALES" ? "bg-blue-50 text-blue-600" : row.objective === "AWARENESS" ? "bg-purple-50 text-purple-600" : row.objective === "TRAFFIC" ? "bg-orange-50 text-orange-600" : row.objective === "LEADS" ? "bg-emerald-50 text-emerald-600" : "bg-surface-container-high text-on-surface-variant"}`}>{row.objective}</span>
    </td>
    <td className="px-6 py-4 text-body-sm text-on-surface font-medium">${row.budget?.toLocaleString() || "0"}</td>
    <td className="px-6 py-4">
      <div className="flex items-center gap-2">
        <span className="text-body-sm text-on-surface font-medium">${row.spend.toLocaleString()}</span>
        {row.budget && row.budget > 0 && (
          <span className="text-label-sm text-outline">({Math.round(row.spend / row.budget * 100)}%)</span>
        )}
      </div>
    </td>
    <td className="px-6 py-4">
      <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-label-sm font-semibold ${
        row.status === "ACTIVE" ? "bg-emerald-50 text-emerald-600" : row.status === "COMPLETED" ? "bg-blue-50 text-blue-600" : "bg-surface-container-high text-on-surface-variant"
      }`}>
        <span className={`w-1.5 h-1.5 rounded-full ${row.status === "ACTIVE" ? "bg-emerald-500 animate-pulse" : "bg-outline"}`} />
        {row.status === "ACTIVE" ? "Active" : row.status === "COMPLETED" ? "Completed" : row.status === "PAUSED" ? "Paused" : "Draft"}
      </span>
    </td>
  </tr>
))}
```

- [ ] **Step 3: Build FE to verify**

```powershell
npm run build --prefix AISAM-FE
```
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add AISAM-FE/src/app/\(dashboard\)/dashboard/page.tsx
git commit -m "feat: connect dashboard campaigns table to real API"
```

---

### Task 10: Connect brands/[id] campaigns tab and analytics to API

**Files:**
- Modify: `AISAM-FE\src\app\(dashboard)\brands\[id]\page.tsx`
- Modify: `AISAM-FE\src\services\analyticsService.ts`

- [ ] **Step 1: Update brands/[id] campaigns tab to fetch from API**

In `AISAM-FE\src\app\(dashboard)\brands\[id]\page.tsx`, find the campaigns tab section.

Add import:
```typescript
import { fetchCampaigns, type Campaign } from "@/services/campaignService";
```

In the useEffect that loads brand data, add a campaign fetch filtered by brand:
```typescript
// Inside the loadBrands effect or a separate useEffect:
fetchCampaigns({ pageSize: 100 }).then((res) => {
  setCampaigns(res.data.filter((c) => c.brandId === brandId));
});
```

Update the `Campaign` interface used in that file to match the one from campaignService, or import it directly (remove the local `Campaign` interface definition at lines 29-40 that shadows the import).

- [ ] **Step 2: Update analyticsService.ts to use real campaign performance**

In `AISAM-FE\src\services\analyticsService.ts`, update the placeholder that returns `campaignPerformance: []`. The analytics dashboard can use `fetchCampaigns()` directly - no change to analyticsService.ts needed if we update the analytics page component instead.

In `AISAM-FE\src\app\(dashboard)\analytics\page.tsx`, update the campaign performance section to call `fetchCampaigns()` instead of `analyticsService.getDashboardData()`.

- [ ] **Step 3: Build FE to verify**

```powershell
npm run build --prefix AISAM-FE
```
Expected: Build succeeds.

- [ ] **Step 4: Commit**

```bash
git add AISAM-FE/src/app/\(dashboard\)/brands/\[id\]/page.tsx AISAM-FE/src/services/analyticsService.ts AISAM-FE/src/app/\(dashboard\)/analytics/page.tsx
git commit -m "feat: connect brand campaigns tab and analytics to API"
```

---

### Task 11: Final verification - run both BE and FE

- [ ] **Step 1: Run BE and verify Swagger endpoints**

```powershell
dotnet run --project AISAM-BE/AISAM.API
```

Open `http://localhost:5116/swagger` and verify:
- `GET /api/campaigns` appears
- `POST /api/campaigns` appears
- `GET /api/campaigns/{id}` appears
- `PUT /api/campaigns/{id}` appears
- `DELETE /api/campaigns/{id}` appears
- `POST /api/campaigns/{id}/restore` appears

Ctrl+C to stop.

- [ ] **Step 2: Run FE dev server and verify no errors**

```powershell
npm run dev --prefix AISAM-FE
```

Open `http://localhost:3000/campaigns` and verify:
- Page loads without errors
- If no campaigns exist, empty state shows
- Create campaign modal works (brands loaded from API)
- All other pages (dashboard, analytics, brands) load without errors

- [ ] **Step 3: Commit any final fixes**

```bash
git add -A
git commit -m "chore: final verification fixes for campaigns feature"
```

---

## Summary

This plan implements the full Campaigns feature across 11 tasks:

| # | Task | Layer | Type |
|---|------|-------|------|
| 1 | Add ManageCampaigns permission enum | BE | Create |
| 2 | Campaign DTOs (request + response) | BE | Create |
| 3 | AdCampaign repository (interface + impl) | BE | Create |
| 4 | AdCampaign service (interface + impl) | BE | Create |
| 5 | AdCampaign controller (6 endpoints) | BE | Create |
| 6 | Register DI + middleware protection | BE | Modify |
| 7 | Rewrite campaignService.ts (API calls) | FE | Modify |
| 8 | Remove hardcoded brands, connect to API | FE | Modify |
| 9 | Dashboard campaigns table → API | FE | Modify |
| 10 | Brand campaigns tab + analytics → API | FE | Modify |
| 11 | Final verification | Both | Verify |

**API Endpoints created:**
- `GET /api/campaigns` - List (paged, searchable, sortable)
- `GET /api/campaigns/{id}` - Get by ID
- `POST /api/campaigns` - Create
- `PUT /api/campaigns/{id}` - Update
- `DELETE /api/campaigns/{id}` - Soft delete
- `POST /api/campaigns/{id}/restore` - Restore
