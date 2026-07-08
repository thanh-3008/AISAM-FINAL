using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/tools")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminToolsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IContentRepository _contentRepository;
    private readonly AisamContext _context;
    private readonly ILogger<AdminToolsController> _logger;

    public AdminToolsController(
        IUserRepository userRepository,
        IWorkspaceRepository workspaceRepository,
        IContentRepository contentRepository,
        AisamContext context,
        ILogger<AdminToolsController> logger)
    {
        _userRepository = userRepository;
        _workspaceRepository = workspaceRepository;
        _contentRepository = contentRepository;
        _context = context;
        _logger = logger;
    }

    [HttpPost("seed-demo-users")]
    public async Task<ActionResult<GenericResponse<object>>> SeedDemoUsers([FromQuery] int count = 5, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var created = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var email = $"demo{i + 1}@aisam-demo.com";
            var exists = await _userRepository.GetByEmailAsync(email);
            if (exists != null) continue;

            using var hmac = new HMACSHA512();
            var user = new User
            {
                Email = email,
                FullName = $"Demo User {i + 1}",
                Role = UserRoleEnum.User,
                IsEmailVerified = true,
                PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes("Demo@123"))),
                PasswordSalt = Convert.ToBase64String(hmac.Key)
            };
            await _userRepository.CreateAsync(user);
            created.Add(email);
        }

        return Ok(GenericResponse<object>.CreateSuccess(new { Created = created, Count = created.Count }));
    }

    [HttpPost("seed-demo-content")]
    public async Task<ActionResult<GenericResponse<object>>> SeedDemoContent([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var workspaces = await _workspaceRepository.GetAllActiveAsync(cancellationToken);
        if (workspaces.Count == 0)
            return Ok(GenericResponse<object>.CreateError("No active workspaces found. Create workspaces first."));

        var random = new Random();
        var titles = new[] { "Summer Sale", "New Launch", "Flash Deal", "Holiday Special", "Clearance", "Early Bird", "VIP Offer", "Bundle Deal", "Free Trial", "Referral Bonus" };
        var created = 0;

        for (int i = 0; i < count && workspaces.Count > 0; i++)
        {
            var ws = workspaces[random.Next(workspaces.Count)];
            var brand = await _context.Brands
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.WorkspaceId == ws.Id && !b.IsDeleted, cancellationToken);

            if (brand == null) continue;

            var content = new Content
            {
                ProfileId = brand.ProfileId,
                WorkspaceId = ws.Id,
                BrandId = brand.Id,
                AdType = (AdTypeEnum)random.Next(0, 3),
                Title = titles[random.Next(titles.Length)],
                TextContent = $"Demo content #{i + 1} for workspace {ws.Name}. This is auto-generated demo content.",
                Status = (ContentStatusEnum)random.Next(0, 3),
                IsAiGenerated = random.Next(2) == 1
            };
            await _contentRepository.AddAsync(content, cancellationToken);
            created++;
        }

        return Ok(GenericResponse<object>.CreateSuccess(new { Created = created }));
    }
}
