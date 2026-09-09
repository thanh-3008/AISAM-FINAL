using AISAM.API.Utils;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers;
[ApiController,Authorize,Route("api/team/member-performance")]
public sealed class MemberPerformanceController(MemberPerformanceService service):ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery]DateTimeOffset from,[FromQuery]DateTimeOffset to,
        [FromQuery]Guid? brandId=null,[FromQuery]Guid? teamId=null,[FromQuery]Guid? memberId=null,
        [FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)
    {
        try { return Ok(new {success=true,data=await service.GetAsync(UserClaimsHelper.GetUserIdOrThrow(User),WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(HttpContext),
            from.UtcDateTime,to.UtcDateTime,brandId,teamId,memberId,page,pageSize,ct)}); }
        catch(ArgumentException e) {return BadRequest(new {success=false,message=e.Message});}
        catch(PerformanceAccessException e) {return StatusCode(e.StatusCode,new {success=false,errorCode="RESOURCE_ACCESS_DENIED"});}
    }
}
