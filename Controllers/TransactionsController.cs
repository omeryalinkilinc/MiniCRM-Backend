using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;
using System.Security.Claims;

namespace MiniCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly MiniCRMDbContext _context;

        public TransactionsController(MiniCRMDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _context.Transactions.ToListAsync();
            return Ok(transactions);
        }

        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.CustomerId == customerId)
                .Include(t => t.Customer)
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Model hatası - {key}: {error.ErrorMessage}");
                    }
                }

                return BadRequest(ModelState);
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return Ok(transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Transaction updated)
        {
            var existing = await _context.Transactions.FindAsync(id);
            if (existing == null) return NotFound();

            existing.TransactionType = updated.TransactionType;
            existing.Description = updated.Description;
            existing.Date = updated.Date;
            existing.Status = updated.Status;
            existing.Amount = updated.Amount;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent()
        {
            var recentTransactions = await _context.Transactions
                .Include(t => t.Customer)
                .OrderByDescending(t => t.Date)
                .Take(6)
                .ToListAsync();

            return Ok(recentTransactions);
        }
    }
}
