using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProducts.Core.Entities;
using XProducts.Core.Interfaces;
using XProducts.Infrastructure.Data;
using XProducts.Infrastructure.Repositories;

namespace ProductOrderingAPI.Test
{
    public class ProductRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Product()
        {
            var ctx = CreateDbContext();
            var repo = new ProductRepository(ctx);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Phone",
                Price = 200,
                StockQuantity = 5
            };

            await repo.AddAsync(product);
            await ctx.SaveChangesAsync();

            Assert.Equal(1, ctx.Products.Count());
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Products()
        {
            var ctx = CreateDbContext();
            var repo = new ProductRepository(ctx);

            ctx.Products.Add(new Product { Id = Guid.NewGuid(), Name = "A", Price = 50, StockQuantity = 10 });
            ctx.Products.Add(new Product { Id = Guid.NewGuid(), Name = "B", Price = 100, StockQuantity = 20 });
            await ctx.SaveChangesAsync();

            var list = await repo.GetAllAsync();

            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Product()
        {
            var ctx = CreateDbContext();
            var repo = new ProductRepository(ctx);

            var id = Guid.NewGuid();
            ctx.Products.Add(new Product { Id = id, Name = "Mouse", Price = 25, StockQuantity = 15 });
            await ctx.SaveChangesAsync();

            var p = await repo.GetByIdAsync(id);

            Assert.NotNull(p);
            Assert.Equal("Mouse", p!.Name);
        }

        [Fact]
        public async Task Update_Should_Modify_Product()
        {
            var ctx = CreateDbContext();
            var repo = new ProductRepository(ctx);

            var product = new Product { Id = Guid.NewGuid(), Name = "Tablet", Price = 300, StockQuantity = 10 };
            ctx.Products.Add(product);
            await ctx.SaveChangesAsync();

            product.Price = 350;
            repo.Update(product);
            await ctx.SaveChangesAsync();

            var updated = await ctx.Products.FindAsync(product.Id);
            Assert.Equal(350, updated!.Price);
        }

        [Fact]
        public async Task Remove_Should_Delete_Product()
        {
            var ctx = CreateDbContext();
            var repo = new ProductRepository(ctx);

            var product = new Product { Id = Guid.NewGuid(), Name = "Laptop", Price = 1000, StockQuantity = 3 };
            ctx.Products.Add(product);
            await ctx.SaveChangesAsync();

            repo.Remove(product);
            await ctx.SaveChangesAsync();

            Assert.Empty(ctx.Products);
        }

        [Fact]
        public async Task GetByIdForUpdateAsync_Should_Return_Product_Using_Raw_SQL()
        {
            var ctx = CreateDbContext();
            var repo = new ProductRepository(ctx);

            var id = Guid.NewGuid();
            ctx.Products.Add(new Product
            {
                Id = id,
                Name = "Camera",
                Price = 500,
                StockQuantity = 7
            });

            await ctx.SaveChangesAsync();

            var result = await repo.GetByIdForUpdateAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Camera", result!.Name);
        }
    }
}
