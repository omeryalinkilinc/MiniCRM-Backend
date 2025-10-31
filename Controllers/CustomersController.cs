using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Data;
using MiniCRM.Api.Models;

namespace MiniCRM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly MiniCRMDbContext _context;

        public CustomersController(MiniCRMDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _context.Customers
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        Email = c.User.Email,
                        c.Company,
                        c.CustomerType,
                        c.RegistrationDate,
                        TransactionCount = _context.Transactions.Count(t => t.CustomerId == c.Id)
                    })
                    .ToListAsync();

                return Ok(customers);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetCustomers hatasý: " + ex.Message);
                return StatusCode(500, "Sunucu hatasý: " + ex.Message);
            }
        }




        // POST: api/customers
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (customer == null)
                return BadRequest("Müþteri verisi eksik.");

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(customer); // Ýsteðe baðlý: eklenen veriyi geri dönebilir
        }



        // PUT: api/customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer updatedCustomer)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.Name = updatedCustomer.Name;
            customer.Company = updatedCustomer.Company;
            customer.TransactionCount = updatedCustomer.TransactionCount;
            customer.CustomerType = updatedCustomer.CustomerType;
            customer.RegistrationDate = updatedCustomer.RegistrationDate;

            await _context.SaveChangesAsync();
            return Ok(customer);
        }



        // DELETE: api/customers/{id}

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if(customer == null) return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return Ok(customer);
        }



        // GET: api/customers/customer-segments

        [HttpGet("customer-segments")]
        public IActionResult GetCustomerSegments()
        {
            Console.WriteLine("GetCustomerSegments endpoint triggered");
            var segments = _context.Customers
                .GroupBy(c => c.CustomerType)
                .Select(g => new {
                    name = g.Key,
                    value = g.Count()
                }).ToList();

            return Ok(segments);
        }


        //GET: api/customers/count

        [HttpGet("count")]
        public async Task<IActionResult> GetCustomerCount()
        {
            var count = await _context.Customers.CountAsync();
            return Ok(new {total = count});
        }


        [HttpGet("transaction-count")]
        public async Task<IActionResult> GetTransactionCount()
        {
            int total = await _context.Transactions.CountAsync();
            return Ok(new { total });
        }







    }
}
