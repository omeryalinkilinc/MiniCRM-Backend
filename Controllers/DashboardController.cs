using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;
using System.Security.Claims;


[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly MiniCRMDbContext _context;

    public DashboardController(MiniCRMDbContext context)
    {
        _context = context;
    }
    [HttpGet("monthly-customer-count")]
    public IActionResult GetMonthlyCustomerCount()
    {
        var customers = _context.Customers.ToList();

        var result = customers
            .GroupBy(c => new { c.RegistrationDate.Year, c.RegistrationDate.Month })
            .Select(g => new MonthlyCustomerCountDto
            {
                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM", new CultureInfo("tr-TR")),
                Count = g.Sum(c => c.TransactionCount ?? 0)
                // ⬅️ işlem sayısını topla
            })
            .OrderBy(x => x.Month)
            .ToList();

        return Ok(result);
    }


    [HttpGet("customer-growth")]
    public IActionResult GetCustomerGrowth()
    {
        var now = DateTime.UtcNow;
        var currentMonth = now.Month;
        var currentYear = now.Year;
        var previousMonth = now.AddMonths(-1).Month;
        var previousYear = now.AddMonths(-1).Year;

        var currentCount = _context.Customers
            .Count(c => c.RegistrationDate.Month == currentMonth && c.RegistrationDate.Year == currentYear);

        var previousCount = _context.Customers
            .Count(c => c.RegistrationDate.Month == previousMonth && c.RegistrationDate.Year == previousYear);

        Console.WriteLine($"Bu ayki müşteri sayısı: {currentCount}");
        Console.WriteLine($"Geçen ayki müşteri sayısı: {previousCount}");

        var growth = previousCount == 0
            ? (currentCount > 0 ? 100 : 0)
            : ((double)(currentCount - previousCount) / previousCount) * 100;

        return Ok(Math.Round(growth, 2));
    }


    [HttpGet("monthly-transaction-volume")]
    public async Task<IActionResult> GetMonthlyTransactionVolume()
    {
        try
        {
            var transactions = await _context.Transactions
                .Where(t => t.Date > DateTime.MinValue)
                .ToListAsync();

            var monthlyData = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new
                {
                    month = new DateTime(g.Key.Year, g.Key.Month, 1)
                        .ToString("MMMM yyyy", new CultureInfo("tr-TR")), // ✅ örnek: "Ekim 2025"
                    count = g.Count()
                })
                .OrderBy(x => x.month)
                .ToList();

            return Ok(monthlyData);
        }
        catch (Exception ex)
        {
            Console.WriteLine("monthly-transaction-volume hatası: " + ex.Message);
            return StatusCode(500, "Sunucu hatası: " + ex.Message);
        }
    }











}




