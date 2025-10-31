using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Models;

namespace MiniCRM.Api.Data
{
    public class MiniCRMDbContext : DbContext
    {
        public MiniCRMDbContext(DbContextOptions<MiniCRMDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; } = default!;

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Message> Messages { get; set; }


    }
}