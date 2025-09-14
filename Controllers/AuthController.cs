using Microsoft.AspNetCore.Mvc;
using MiniCRM.Api.Models;

namespace MiniCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Email == "demo@minicrm.com" && request.Password == "123456")
            {
                return Ok(new { token = "demo-token-xyz" });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}
