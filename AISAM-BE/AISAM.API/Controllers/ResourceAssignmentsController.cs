using AISAM.API.Utils;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AISAM.API.Controllers;

[ApiController, Authorize]
public sealed class ResourceAssignmentsController(AssignmentService assignments,AISAM.Repositories.AisamContext db) : ControllerBase
{
    public sealed record ChangeRequest(string ExpectedRevision, bool CanView=false, bool CanPublish=false, bool CanManage=false);
    private Guid Actor => UserClaimsHelper.GetUserIdOrThrow(User);
    private Guid Workspace => WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
    [HttpGet("api/teams")]
    public async Task<IActionResult> Teams([FromQuery]int page=1,[FromQuery]int pageSize=50,CancellationToken ct=default)
    {
        var query=db.Teams.AsNoTracking().Where(t=>t.WorkspaceId==Workspace && !t.IsDeleted);
        var count=await query.CountAsync(ct);
        var items=await query.OrderBy(t=>t.Name).ThenBy(t=>t.Id).Skip((Math.Max(1,page)-1)*Math.Clamp(pageSize,1,100)).Take(Math.Clamp(pageSize,1,100))
            .Select(t=>new {t.Id,t.Name,t.Status}).ToListAsync(ct);
        return Ok(new {success=true,statusCode=200,data=new {items,totalCount=count}});
    }
    private static object View(AssignmentSnapshot snapshot) => new {
        revision=snapshot.Revision,
        teams=snapshot.Teams.Select(t=>new {t.Id,t.TeamId,t.IsActive}),
        channels=snapshot.Channels.Select(c=>new {c.TeamBrandId,c.IntegrationId,c.CanView,c.CanPublish,c.CanManage})
    };
    private async Task<IActionResult> Execute(Func<Task<AssignmentSnapshot>> operation)
    {
        try { return Ok(new { data=View(await operation()),statusCode=200,success=true }); }
        catch(AssignmentAccessException e) { return StatusCode(e.Decision.StatusCode,new {success=false,errorCode=e.Decision.ErrorCode}); }
        catch(AssignmentConflictException) { return Conflict(new {success=false,errorCode="ACCESS_REVISION_CONFLICT"}); }
        catch(InvalidOperationException e) { return BadRequest(new {success=false,errorCode=e.Message,message=e.Message}); }
        catch(ArgumentException e) { return BadRequest(new {success=false,message=e.Message}); }
        catch(Exception e) when ((e as PostgresException ?? (e as DbUpdateException)?.InnerException as PostgresException)?.SqlState is "40001" or "23505")
        { return Conflict(new {success=false,errorCode="ACCESS_REVISION_CONFLICT"}); }
    }
    [HttpGet("api/brands/{brandId:guid}/access")]
    public Task<IActionResult> Read(Guid brandId,CancellationToken ct) => Execute(()=>assignments.ReadAsync(Actor,Workspace,brandId,ct));
    [HttpPut("api/brands/{brandId:guid}/teams/{teamId:guid}")]
    public Task<IActionResult> Grant(Guid brandId,Guid teamId,ChangeRequest request,CancellationToken ct) =>
        Execute(()=>assignments.ChangeAsync(new(Actor,Workspace,brandId,teamId,request.ExpectedRevision,true),ct));
    [HttpDelete("api/brands/{brandId:guid}/teams/{teamId:guid}")]
    public Task<IActionResult> Revoke(Guid brandId,Guid teamId,[FromHeader(Name="If-Match")]string revision,CancellationToken ct) =>
        Execute(()=>assignments.ChangeAsync(new(Actor,Workspace,brandId,teamId,revision,false),ct));
    [HttpPut("api/brands/{brandId:guid}/channels/{integrationId:guid}/teams/{teamId:guid}")]
    public Task<IActionResult> GrantChannel(Guid brandId,Guid integrationId,Guid teamId,ChangeRequest request,CancellationToken ct) =>
        Execute(()=>assignments.ChangeAsync(new(Actor,Workspace,brandId,teamId,request.ExpectedRevision,true,integrationId,request.CanView,request.CanPublish,request.CanManage),ct));
    [HttpDelete("api/brands/{brandId:guid}/channels/{integrationId:guid}/teams/{teamId:guid}")]
    public Task<IActionResult> RevokeChannel(Guid brandId,Guid integrationId,Guid teamId,[FromHeader(Name="If-Match")]string revision,CancellationToken ct) =>
        Execute(()=>assignments.ChangeAsync(new(Actor,Workspace,brandId,teamId,revision,false,integrationId),ct));
}
