using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;

namespace MiniCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly MiniCRMDbContext _context;

        public ReportsController(MiniCRMDbContext context)
        {
            _context = context;
        }

        [HttpGet("new-customers")]
        public async Task<IActionResult> GetNewCustomersReport()
        {
            var now = DateTime.UtcNow;
            var oneWeekAgo = now.AddDays(-7);

            var weeklyList = await _context.Customers
                .Where(c => c.RegistrationDate >= oneWeekAgo)
                .ToListAsync();

            var monthlyList = await _context.Customers
                .Where(c => c.RegistrationDate.Month == now.Month &&
                            c.RegistrationDate.Year == now.Year)
                .ToListAsync();

            var result = new
            {
                WeeklyCount = weeklyList.Count,
                MonthlyCount = monthlyList.Count,
                WeeklyList = weeklyList,
                MonthlyList = monthlyList
            };

            return Ok(result);
        }
    }
}
