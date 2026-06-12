using AISAM.API.Utils;
using AISAM.Common;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Services.IServices;
using AISAM.Data.Enumeration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
public sealed class WorkspaceController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspaceController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    public async Task<ActionResult<GenericResponse<IReadOnlyList<WorkspaceResponseDto>>>> GetMine(
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceService.GetByUserIdAsync(userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GenericResponse<WorkspaceResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceService.GetByIdAsync(id, userId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<GenericResponse<WorkspaceResponseDto>>> Create(
        [FromBody] CreateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceService.CreateAsync(userId, request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GenericResponse<WorkspaceResponseDto>>> Update(
        Guid id,
        [FromBody] UpdateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceService.UpdateAsync(id, userId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRoleEnum.Admin))]
    public async Task<ActionResult<GenericResponse<bool>>> AdminSoftDelete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = UserClaimsHelper.GetUserIdOrThrow(User);
        var result = await _workspaceService.AdminSoftDeleteAsync(id, adminUserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
