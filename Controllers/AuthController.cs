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

            // Access token üret
            var accessToken = _jwtService.GenerateToken(user);

            // Refresh token üret
            var refreshToken = _jwtService.GenerateRefreshToken(user.Id);

            // Veritabanına kaydet
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // Refresh token'ı cookie olarak gönder
            Response.Cookies.Append("refresh_token", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // localhost için
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = refreshToken.Expires
            });

            // Access token'ı cookie olarak da gönderebilirsin (isteğe bağlı)
            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

            // Access token'ı JSON olarak da döndür
            return Ok(new
            {
                message = "Giriş başarılı",
                token = accessToken,
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
        public async Task<IActionResult> Logout()
        {
            var refreshTokenValue = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshTokenValue))
            {
                var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshTokenValue);
                if (token != null)
                {
                    token.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }
            }

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            Console.WriteLine("Oturum sonlandırıldı");
            return Ok(new { message = "Çıkış başarılı" });
        }




        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshTokenValue = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshTokenValue))
                return Unauthorized("Refresh token bulunamadı");

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);

            if (storedToken == null || storedToken.Expires < DateTime.UtcNow || storedToken.IsRevoked)
                return Unauthorized("Refresh token geçersiz");

            var user = storedToken.User;
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı");

            // Yeni access token üret
            var newAccessToken = _jwtService.GenerateToken(user);

            // Yeni refresh token üret
            var newRefreshToken = _jwtService.GenerateRefreshToken(user.Id);

            // Eski token'ı iptal et
            storedToken.IsRevoked = true;

            // Yeni token'ı veritabanına ekle
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            // Yeni refresh token'ı cookie olarak gönder
            Response.Cookies.Append("refresh_token", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = newRefreshToken.Expires
            });

            // Yeni access token'ı cookie olarak da gönder
            Response.Cookies.Append("access_token", newAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

            return Ok(new
            {
                message = "Token yenilendi",
                token = newAccessToken,
                fullName = user.FullName,
                role = user.Role
            });
        }




    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
