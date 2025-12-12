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

namespace ProductOrderingAPI.Test
{
    public class OrderServiceTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task PlaceOrder_ShouldCreateOrder_WhenStockIsAvailable()
        {
            // Arrange
            var db = CreateDbContext(nameof(PlaceOrder_ShouldCreateOrder_WhenStockIsAvailable));
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop",
                Price = 1000,
                StockQuantity = 10
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var service = new OrderService(db, mockRepo.Object);

            // Act
            var order = await service.PlaceOrderAsync(
                new[] { (product.Id, 2) }
            );

            // Assert
            Assert.NotNull(order);
            Assert.Single(order.Items);
            Assert.Equal(2000, order.Total);
            Assert.Equal(8, db.Products.Single().StockQuantity);
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenProductDoesNotExist()
        {
            var db = CreateDbContext(nameof(PlaceOrder_ShouldThrow_WhenProductDoesNotExist));
            var mockRepo = new Mock<IProductRepository>();

            var service = new OrderService(db, mockRepo.Object);

            var missingId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceOrderAsync(new[] { (missingId, 1) })
            );
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenStockIsInsufficient()
        {
            var db = CreateDbContext(nameof(PlaceOrder_ShouldThrow_WhenStockIsInsufficient));
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Phone",
                Price = 500,
                StockQuantity = 1
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var service = new OrderService(db, mockRepo.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceOrderAsync(new[] { (product.Id, 5) })
            );
        }

        [Fact]
        public async Task PlaceOrder_ShouldDecreaseStock_OnSuccess()
        {
            var db = CreateDbContext(nameof(PlaceOrder_ShouldDecreaseStock_OnSuccess));
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Keyboard",
                Price = 100,
                StockQuantity = 5
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var service = new OrderService(db, mockRepo.Object);

            var order = await service.PlaceOrderAsync(new[] { (product.Id, 3) });

            Assert.Equal(2, db.Products.Single().StockQuantity);
        }

        [Fact]
        public async Task PlaceOrder_ShouldRetryOnConcurrencyException()
        {
            var db = CreateDbContext(nameof(PlaceOrder_ShouldRetryOnConcurrencyException));
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Monitor",
                Price = 150,
                StockQuantity = 10
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var service = new OrderService(db, mockRepo.Object);

            // Inject concurrency exception on first SaveChanges
            int callCount = 0;
            db.SavingChanges += (sender, args) =>
            {
                callCount++;
                if (callCount == 1)
                    throw new DbUpdateConcurrencyException();
            };

            var order = await service.PlaceOrderAsync(new[] { (product.Id, 2) });

            Assert.Equal(8, db.Products.Single().StockQuantity);
            Assert.Equal(2, callCount); // retried once
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenConcurrencyRetriesExceedLimit()
        {
            var db = CreateDbContext(nameof(PlaceOrder_ShouldThrow_WhenConcurrencyRetriesExceedLimit));
            var mockRepo = new Mock<IProductRepository>();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Tablet",
                Price = 300,
                StockQuantity = 5
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var service = new OrderService(db, mockRepo.Object);

            db.SavingChanges += (sender, args) =>
            {
                throw new DbUpdateConcurrencyException();
            };

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                service.PlaceOrderAsync(new[] { (product.Id, 1) })
            );
        }

    }
}
