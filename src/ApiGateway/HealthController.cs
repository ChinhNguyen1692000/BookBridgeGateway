using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [ApiController]
    // Sử dụng route chính xác mà Render đang tìm kiếm
    [Route("api/[controller]")] 
    public class HealthController : ControllerBase
    {
        [HttpGet]
        [Route("healthz")]
        public IActionResult GetHealth()
        {
            // Trả về kết quả 200 OK
            return Ok("Healthy"); 
        }
    }
}