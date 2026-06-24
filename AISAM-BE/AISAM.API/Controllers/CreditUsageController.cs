using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/credit-usage")]
[Authorize]
public sealed class CreditUsageController : ControllerBase
{
    private readonly ICreditService _creditService;

    public CreditUsageController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    [HttpGet("wallet")]
    public async Task<ActionResult<GenericResponse<object>>> GetWallet(CancellationToken cancellationToken = default)
    {
        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var wallet = await _creditService.GetWalletAsync(workspaceId, cancellationToken);
        if (wallet == null)
        {
            return Ok(GenericResponse<object>.CreateSuccess(new { balance = 0, workspaceId }));
        }
        return Ok(GenericResponse<object>.CreateSuccess(new { balance = wallet.Balance, workspaceId }));
    }

    [HttpGet("daily-summary")]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<DailyCreditUsageDto>>>> GetDailySummary(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (days is not 7 and not 30 and not 90)
        {
            days = 30;
        }

        var workspaceId = WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
        var result = await _creditService.GetDailyUsageAsync(workspaceId, days, cancellationToken);
        return Ok(GenericResponse<IReadOnlyList<DailyCreditUsageDto>>.CreateSuccess(result));
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<PagedResult<CreditUsageRecordDto>>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _creditService.GetPagedUsageAsync(
            WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            new PaginationRequest { Page = page, PageSize = pageSize },
            cancellationToken);

        return Ok(GenericResponse<PagedResult<CreditUsageRecordDto>>.CreateSuccess(result));
    }
}
