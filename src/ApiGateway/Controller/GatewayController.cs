using Microsoft.AspNetCore.Mvc;

namespace BookService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayController : ControllerBase
    {
        [HttpGet("/api/healthz")] // <-- Change: Explicitly set the absolute path
        public IActionResult HealthCheck() => Ok("Healthy");
    }
}
