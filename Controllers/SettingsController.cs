using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;
using System.Security.Claims;
using BCrypt.Net; // ✅ BCrypt.Net paketini kullanıyoruz

namespace MiniCRM.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly MiniCRMDbContext _context;

        public SettingsController(MiniCRMDbContext context)
        {
            _context = context;
        }

        // Kullanıcı ayarlarını getir
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // FullName'i güvenli şekilde parçala
            var parts = (user.FullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.Length > 0 ? parts[0] : string.Empty;
            var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;

            return Ok(new
            {
                firstName = firstName,
                lastName = lastName,
                email = user.Email ?? string.Empty,
                photoUrl = user.PhotoUrl ?? string.Empty,
                phone = user.Phone ?? string.Empty,
                companyName = user.CompanyName ?? string.Empty
            });
        }

        // Kullanıcı ayarlarını güncelle
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Güncelle
            var first = dto.FirstName?.Trim() ?? string.Empty;
            var last = dto.LastName?.Trim() ?? string.Empty;
            user.FullName = string.IsNullOrWhiteSpace(last) ? first : $"{first} {last}";
            user.Email = dto.Email ?? user.Email;
            user.PhotoUrl = dto.PhotoUrl ?? user.PhotoUrl;
            user.Phone = dto.Phone ?? user.Phone;
            user.CompanyName = dto.CompanyName ?? user.CompanyName;

            await _context.SaveChangesAsync();

            // Frontend ile uyumlu response
            return Ok(new
            {
                firstName = user.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
                lastName = user.FullName.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                email = user.Email ?? string.Empty,
                photoUrl = user.PhotoUrl ?? string.Empty,
                phone = user.Phone ?? string.Empty,
                companyName = user.CompanyName ?? string.Empty
            });
        }

        // Şifre değiştirme
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = int.Parse(userIdStr);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Mevcut şifre kontrolü
            if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return BadRequest("Mevcut şifre yanlış");

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Şifre güncellendi" });
        }

        // ✅ BCrypt helper metotları
        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // DTO
        public class ChangePasswordDto
        {
            public string CurrentPassword { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }
    }

    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Phone { get; set; }
        public string? CompanyName { get; set; }
    }
}
