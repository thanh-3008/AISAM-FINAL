using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Data.Enumeration;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/admin/credit-oversight")]
[Authorize(Roles = nameof(UserRoleEnum.Admin))]
public sealed class AdminCreditController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IAiGenerationRepository _aiGenerationRepository;
    private readonly ICreditUsageRecordRepository _creditUsageRepository;
    private readonly ICreditService _creditService;

    public AdminCreditController(
        IUserRepository userRepository,
        IAiGenerationRepository aiGenerationRepository,
        ICreditUsageRecordRepository creditUsageRepository,
        ICreditService creditService)
    {
        _userRepository = userRepository;
        _aiGenerationRepository = aiGenerationRepository;
        _creditUsageRepository = creditUsageRepository;
        _creditService = creditService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GenericResponse<object>>> GetSummary(CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        var now = DateTime.UtcNow;
        var from7 = now.AddDays(-7);
        var from30 = now.AddDays(-30);

        var totalGenerations = await _aiGenerationRepository.GetTotalGenerationCountAsync(cancellationToken);
        var recentDaily = await _aiGenerationRepository.GetDailyGenerationCountAsync(from7, now, cancellationToken);

        var dailyAiData = new List<object>();
        for (int i = 6; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            dailyAiData.Add(new { name = date.ToString("ddd"), generations = recentDaily.GetValueOrDefault(date, 0) });
        }

        int estimatedCreditSpent = 0;
        try
        {
            estimatedCreditSpent = await _creditUsageRepository.GetTotalCreditsUsedAsync(cancellationToken);
        }
        catch { }

        return Ok(GenericResponse<object>.CreateSuccess(new
        {
            TotalAiGenerations = totalGenerations,
            WeeklyAiGenerations = recentDaily.Values.Sum(),
            DailyAiData = dailyAiData,
            EstimatedCreditSpent = estimatedCreditSpent,
            EstimatedRevenue = estimatedCreditSpent * 100
        }));
    }

    public class AdjustCreditRequest
    {
        public Guid WorkspaceId { get; set; }
        public long Amount { get; set; }
        public string Reason { get; set; } = "";
    }

    [HttpPost("adjust")]
    public async Task<ActionResult<GenericResponse<object>>> AdjustCredits([FromBody] AdjustCreditRequest request, CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var admin = await _userRepository.GetByIdAsync(adminUserId);
        if (admin?.Role != UserRoleEnum.Admin)
            return StatusCode(403, GenericResponse<object>.CreateError("Unauthorized", System.Net.HttpStatusCode.Forbidden));

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(GenericResponse<object>.CreateError("Reason is required."));

        if (request.Amount == 0)
            return BadRequest(GenericResponse<object>.CreateError("Amount cannot be zero."));

        var result = await _creditService.AdminAdjustCreditsAsync(request.WorkspaceId, adminUserId, request.Amount, request.Reason, cancellationToken);
        if (!result.Success)
            return StatusCode(result.StatusCode, GenericResponse<object>.CreateError(result.Message ?? "Failed to adjust credits"));

        return Ok(GenericResponse<object>.CreateSuccess(result.Data, result.Message));
    }
}
