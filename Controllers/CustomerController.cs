using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;
using System.Security.Claims;

namespace MiniCRM.Api.Controllers
{
    [ApiController]
    [Route("api/customer")]
    [Authorize] // 🔐 Hem Customer hem Admin erişebilsin
    public class CustomerController : ControllerBase
    {
        private readonly MiniCRMDbContext _context;

        public CustomerController(MiniCRMDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine("Kullanıcı ID: " + userId);
            Console.WriteLine("Kullanıcı rolü: " + role);

            if (role == "Customer")
            {
                var customer = _context.Customers.FirstOrDefault(c => c.UserId == int.Parse(userId));
                if (customer == null) return NotFound();

                return Ok(new
                {
                    id = customer.Id, // ✅ eklendi
                    name = customer.Name,
                    photoUrl = customer.PhotoUrl
                });
            }

            if (role == "Admin")
            {
                var admin = _context.Users.FirstOrDefault(a => a.Id == int.Parse(userId));
                if (admin == null) return NotFound();

                return Ok(new
                {
                    id = admin.Id, // ✅ eklendi
                    name = admin.FullName,
                    photoUrl = admin.PhotoUrl
                });
            }

            return Forbid(); // ❌ Tanımsız rol varsa erişim engellenir
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetCustomerInfo()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            Console.WriteLine("GetCustomerInfo çağrısı - userId: " + userId);

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == int.Parse(userId));

                if (customer == null) return NotFound();

                return Ok(new
                {
                    id = customer.Id, // ✅ eklendi
                    customerType = customer.CustomerType,
                    companyName = customer.Company,
                    photoUrl = customer.PhotoUrl
                });
            }

            if (role == "Admin")
            {
                var admin = await _context.Users
                    .FirstOrDefaultAsync(a => a.Id == int.Parse(userId));

                if (admin == null) return NotFound();

                return Ok(new
                {
                    id = admin.Id, // ✅ eklendi
                    customerType = "Admin",
                    companyName = "Yönetici Paneli",
                    photoUrl = admin.PhotoUrl
                });
            }

            return Forbid(); // ❌ Tanımsız rol varsa erişim engellenir
        }
    }
}
