using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using XProducts.Core.Entities;

namespace XProducts.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(b =>
            {
                b.Property(p => p.Price).HasColumnType("decimal(18,2)");
                b.Property(p => p.RowVersion).IsRowVersion();
            });


            modelBuilder.Entity<Order>(b =>
            {
                b.OwnsMany(o => o.Items);
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}
