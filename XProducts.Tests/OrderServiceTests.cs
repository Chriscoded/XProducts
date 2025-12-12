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
using XProducts.Infrastructure.Services;


namespace XProducts.Tests
{
    public class OrderServiceTests
    {
        private AppDbContext CreateDbContext(out SqliteConnection connection)
        {
            // Create an in-memory SQLite connection
            connection = new SqliteConnection("Filename=:memory:");
            connection.Open(); // keep it open

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated(); // create tables

            return context;
        }


        [Fact]
        public async Task PlaceOrder_ShouldCreateOrder_WhenStockIsAvailable()
        {
            // Arrange
            var context = CreateDbContext(out var connection);
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop",
                Price = 1000,
                StockQuantity = 10
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new OrderService(context, mockRepo.Object);

            // Act
            var order = await service.PlaceOrderAsync(
                new[] { (product.Id, 2) }
            );

            // Assert
            Assert.NotNull(order);
            Assert.Single(order.Items);
            Assert.Equal(2000, order.Total);
            Assert.Equal(8, context.Products.Single().StockQuantity);
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenProductDoesNotExist()
        {
            var context = CreateDbContext(out var connection);
            var mockRepo = new Mock<IProductRepository>();

            var service = new OrderService(context, mockRepo.Object);

            var missingId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceOrderAsync(new[] { (missingId, 1) })
            );
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenStockIsInsufficient()
        {
            var context = CreateDbContext(out var connection);
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Phone",
                Price = 500,
                StockQuantity = 1
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new OrderService(context, mockRepo.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceOrderAsync(new[] { (product.Id, 5) })
            );
        }

        [Fact]
        public async Task PlaceOrder_ShouldDecreaseStock_OnSuccess()
        {
            var context = CreateDbContext(out var connection);
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Keyboard",
                Price = 100,
                StockQuantity = 5
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new OrderService(context, mockRepo.Object);

            var order = await service.PlaceOrderAsync(new[] { (product.Id, 3) });

            Assert.Equal(2, context.Products.Single().StockQuantity);
        }

        [Fact]
        public async Task PlaceOrder_ShouldRetryOnConcurrencyException()
        {
            var context = CreateDbContext(out var connection);
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Monitor",
                Price = 150,
                StockQuantity = 10
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new OrderService(context, mockRepo.Object);

            // Inject concurrency exception on first SaveChanges
            int callCount = 0;
            context.SavingChanges += (sender, args) =>
            {
                callCount++;
                if (callCount == 1)
                    throw new DbUpdateConcurrencyException();
            };

            var order = await service.PlaceOrderAsync(new[] { (product.Id, 2) });

            //Assert.Equal(8, context.Products.Single().StockQuantity);
            Assert.Equal(2, callCount); // retried once
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenConcurrencyRetriesExceedLimit()
        {
            var context = CreateDbContext(out var connection);
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Tablet",
                Price = 300,
                StockQuantity = 5
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new OrderService(context, mockRepo.Object);

            context.SavingChanges += (sender, args) =>
            {
                throw new DbUpdateConcurrencyException();
            };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                service.PlaceOrderAsync(new[] { (product.Id, 1) })
            );
        }

    }
}
