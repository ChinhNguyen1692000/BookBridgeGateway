using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [ApiController]
    // Sử dụng route chính xác mà Render đang tìm kiếm
    [Route("/")] 
    public class HealthController : ControllerBase
    {
        [HttpGet]
        [Route("api/healthz")]
        public IActionResult GetHealth()
        {
            // Trả về kết quả 200 OK
            return Ok("Healthy"); 
        }
    }
}