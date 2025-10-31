using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;
using MiniCRM.Api.Services;

namespace MiniCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MiniCRMDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(MiniCRMDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine("Login tetiklendi");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            Console.WriteLine("Kullanıcı bulundu mu: " + (user != null));

            if (user == null)
                return Unauthorized("Giriş bilgileri hatalı");

            var passwordCheck = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            Console.WriteLine("Şifre doğrulandı mı: " + passwordCheck);

            if (!passwordCheck)
                return Unauthorized("Giriş bilgileri hatalı");

            var token = _jwtService.GenerateToken(user);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
              //  Domain = "localhost",
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // ✅ Role bilgisi eklendi
            return Ok(new
            {
                message = "Giriş başarılı",
                fullName = user.FullName,
                role = user.Role
            });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var fullName = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var userId = User.FindFirstValue("UserId");

            return Ok(new
            {
                message = "Oturum doğrulandı",
                user = new
                {
                    Id = userId,
                    FullName = fullName,
                    Email = email,
                    Role = role
                }
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = "Customer"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var customer = new Customer
            {
                Name = request.FullName,
                Company = request.Company,
                CustomerType = request.CustomerType,
                RegistrationDate = DateTime.Now,
                TransactionCount = 0,
                UserId = user.Id
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt tamamlandı" });
        }


        [HttpGet("generate-hash")]
        public IActionResult GenerateHash()
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("123456");
            Console.WriteLine("Üretilen hash: " + hash);
            return Ok(new { hash });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            Console.WriteLine("Oturum sonlandırıldı");
            return Ok(new { message = "Çıkış başarılı" });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
