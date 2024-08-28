using Microsoft.EntityFrameworkCore;
using NewWeb.Models.Entities;

namespace NewWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Checkout> Checkouts { get; set; }
    }
}
