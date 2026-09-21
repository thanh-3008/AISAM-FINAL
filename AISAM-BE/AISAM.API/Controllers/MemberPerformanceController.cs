using AISAM.API.Utils;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;
[ApiController,Authorize,Route("api/team/member-performance")]
public sealed class MemberPerformanceController(MemberPerformanceService service):ControllerBase
{
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery]DateTimeOffset from,[FromQuery]DateTimeOffset to,
        [FromQuery]Guid? brandId=null,[FromQuery]Guid? teamId=null,[FromQuery]Guid? memberId=null,CancellationToken ct=default)
    {
        try
        {
            var actor=UserClaimsHelper.GetUserIdOrThrow(User);
            var workspace=WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext);
            var first=await service.GetAsync(actor,workspace,from.UtcDateTime,to.UtcDateTime,brandId,teamId,memberId,1,100,false,ct);
            if(first.Total>10000) return BadRequest(new{success=false,message="Narrow the export to at most 10000 members."});
            var rows=first.Items.ToList();
            for(var page=2;(page-1)*100<first.Total;page++)
                rows.AddRange((await service.GetAsync(actor,workspace,from.UtcDateTime,to.UtcDateTime,brandId,teamId,memberId,page,100,false,ct)).Items);
            var result=first with {Items=rows.DistinctBy(r=>r.MemberId).ToArray()};
            return File(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result,new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)),
                "application/json","member-performance.json");
        }
        catch(ArgumentException e) {return BadRequest(new{success=false,message=e.Message});}
        catch(PerformanceAccessException e) {return StatusCode(e.StatusCode,new{success=false,errorCode="RESOURCE_ACCESS_DENIED"});}
    }
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery]DateTimeOffset from,[FromQuery]DateTimeOffset to,
        [FromQuery]Guid? brandId=null,[FromQuery]Guid? teamId=null,[FromQuery]Guid? memberId=null,
        [FromQuery]int page=1,[FromQuery]int pageSize=20,[FromQuery]bool compareTeams=false,CancellationToken ct=default)
    {
        try { return Ok(new {success=true,data=await service.GetAsync(UserClaimsHelper.GetUserIdOrThrow(User),WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            from.UtcDateTime,to.UtcDateTime,brandId,teamId,memberId,page,pageSize,compareTeams,ct)}); }
        catch(ArgumentException e) {return BadRequest(new {success=false,message=e.Message});}
        catch(PerformanceAccessException e) {return StatusCode(e.StatusCode,new {success=false,errorCode="RESOURCE_ACCESS_DENIED"});}
    }
}
