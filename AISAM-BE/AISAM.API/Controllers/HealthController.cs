using AISAM.Common;
using Microsoft.AspNetCore.Mvc;

namespace AISAM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(GenericResponse<object>.CreateSuccess(new
            {
                status = "Healthy",
                service = "AISAM Backend",
                timestamp = DateTime.UtcNow
            }, "AISAM backend is ready."));
        }
    }
}
