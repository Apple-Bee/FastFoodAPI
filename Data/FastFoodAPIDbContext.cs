using Microsoft.EntityFrameworkCore;
using FastFoodAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace FastFoodAPI.Data
{
    public class FastFoodAPIDbContext : IdentityDbContext<IdentityUser>
    {
        public FastFoodAPIDbContext(DbContextOptions<FastFoodAPIDbContext> options)
            : base(options)
        {
        }

        // DbSets representing tables in the database
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }  // This represents the users table

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional custom configurations
            // Example:
            // modelBuilder.Entity<Product>().HasKey(p => p.Id);
        }
    }
}


