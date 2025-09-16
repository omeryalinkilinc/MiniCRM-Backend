using Microsoft.EntityFrameworkCore;
using MiniCRM.Api.Models;

namespace MiniCRM.Api.Data
{
    public class MiniCRMDbContext : DbContext
    {
        public MiniCRMDbContext(DbContextOptions<MiniCRMDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}