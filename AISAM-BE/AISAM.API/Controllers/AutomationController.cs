using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AISAM.Data.Enumeration;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/automation-plans")]
[Authorize]
public sealed class AutomationController : ControllerBase
{
    private readonly IAutomationService _automationService;
    private readonly IProfileRepository _profileRepository;
    private readonly IAutomationApprovalService _automationApprovalService;
    private readonly IWorkspaceMemberRepository _workspaceMemberRepository;
    public AutomationController(IAutomationService automationService, IProfileRepository profileRepository, IAutomationApprovalService automationApprovalService, IWorkspaceMemberRepository workspaceMemberRepository)
    {
        _automationService = automationService;
        _profileRepository = profileRepository;
        _automationApprovalService = automationApprovalService;
        _workspaceMemberRepository = workspaceMemberRepository;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<AutomationPlanDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _automationService.GetAllAsync(GetWorkspaceId(), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{planId:guid}")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> GetById(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _automationService.GetByIdAsync(GetWorkspaceId(), planId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> Create([FromBody] CreateAutomationPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _automationService.CreateAsync(GetWorkspaceId(), await GetProfileIdAsync(cancellationToken), request, cancellationToken: cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("import-csv")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> ImportCsv(
        [FromForm] string name,
        [FromForm] string? timezone,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0 || !string.Equals(Path.GetExtension(file.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(GenericResponse<AutomationPlanDto>.CreateError("A non-empty CSV file is required."));
        await using var stream = file.OpenReadStream();
        var result = await _automationService.ImportCsvAsync(GetWorkspaceId(), await GetProfileIdAsync(cancellationToken), name, timezone ?? "UTC", file.FileName, stream, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{planId:guid}/confirm")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> Confirm(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _automationService.ConfirmAsync(GetWorkspaceId(), planId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{planId:guid}/retry")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> Retry(Guid planId, [FromQuery] Guid? itemId, CancellationToken cancellationToken)
    {
        var result = await _automationService.RetryAsync(GetWorkspaceId(), planId, itemId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{planId:guid}/cancel")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> Cancel(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _automationService.CancelAsync(GetWorkspaceId(), planId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{planId:guid}/approve")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> Approve(Guid planId, [FromQuery] Guid? itemId, CancellationToken cancellationToken)
    {
        var result = await _automationApprovalService.ApproveAsync(GetWorkspaceId(), planId, UserClaimsHelper.GetUserIdOrThrow(User), itemId, cancellationToken: cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{planId:guid}/items/{itemId:guid}/targets")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<AutomationTargetDto>>>> GetTargets(Guid planId, Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _automationApprovalService.GetTargetsAsync(GetWorkspaceId(), planId, itemId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{planId:guid}/items/{itemId:guid}/approve-targets")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> ApproveTargets(Guid planId, Guid itemId, [FromBody] ApproveAutomationTargetsRequest request, CancellationToken cancellationToken)
    {
        var result = await _automationApprovalService.ApproveAsync(GetWorkspaceId(), planId, UserClaimsHelper.GetUserIdOrThrow(User), itemId, request.IntegrationIds, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{planId:guid}/items/{itemId:guid}/reject")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> Reject(Guid planId, Guid itemId, [FromBody] RejectAutomationItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _automationApprovalService.RejectAsync(GetWorkspaceId(), planId, itemId, UserClaimsHelper.GetUserIdOrThrow(User), request.Notes, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("import-google-sheet")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> ImportGoogleSheet([FromBody] ImportGoogleSheetRequest request, CancellationToken cancellationToken)
    {
        var result = await _automationService.ImportGoogleSheetAsync(GetWorkspaceId(), await GetProfileIdAsync(cancellationToken), request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{planId:guid}/clone")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> Clone(Guid planId, [FromBody] CloneAutomationPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _automationService.CloneAsync(GetWorkspaceId(), await GetProfileIdAsync(cancellationToken), planId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{planId:guid}/auto-approve")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> SetAutoApprove(Guid planId, [FromBody] SetAutomationAutoApproveRequest request, CancellationToken cancellationToken)
    {
        var workspaceId = GetWorkspaceId();
        var membership = await _workspaceMemberRepository.GetByWorkspaceAndUserAsync(workspaceId, UserClaimsHelper.GetUserIdOrThrow(User), cancellationToken);
        if (membership?.Role is not WorkspaceMemberRoleEnum.Owner and not WorkspaceMemberRoleEnum.Manager)
            return StatusCode(StatusCodes.Status403Forbidden, GenericResponse<AutomationPlanDto>.CreateError("Only workspace owners and managers can configure auto-approve.", System.Net.HttpStatusCode.Forbidden));
        var result = await _automationService.SetAutoApproveAsync(workspaceId, planId, request.Enabled, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{planId:guid}/performance")]
    public async Task<ActionResult<GenericResponse<AutomationPerformanceDto>>> GetPerformance(Guid planId, CancellationToken cancellationToken)
    {
        var result = await _automationService.GetPerformanceAsync(GetWorkspaceId(), planId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{planId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<GenericResponse<AutomationPlanDto>>> UpdateItem(Guid planId, Guid itemId, [FromBody] UpdateAutomationItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _automationService.UpdateItemAsync(GetWorkspaceId(), planId, itemId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetWorkspaceId() => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
    private Task<Guid> GetProfileIdAsync(CancellationToken cancellationToken)
        => WorkspaceLegacyProfileHelper.GetOrCreateProfileIdAsync(HttpContext, _profileRepository, cancellationToken);
}
